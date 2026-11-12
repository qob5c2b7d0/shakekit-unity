using System.Collections.Generic;
using UnityEngine;

namespace ShakeKit
{
    /// <summary>
    /// Keeps a stack of curve-driven shake layers and applies their sum to a
    /// transform's local pose every LateUpdate, then restores the base pose.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CameraShaker : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(1)] private int maxLayers = 4;
        [SerializeField] private bool useUnscaledTime;
        [SerializeField, Range(0f, 2f)] private float globalScale = 1f;

        private readonly List<Layer> layers = new List<Layer>();
        private Vector3 basePosition;
        private Quaternion baseRotation;
        private bool hasBasePose;

        private struct Layer
        {
            public ShakeProfile Profile;
            public float Age;
            public float Scale;
            public int Seed;
        }

        public int ActiveLayers => layers.Count;

        private void Awake()
        {
            if (target == null)
            {
                target = transform;
            }
        }

        private void OnEnable()
        {
            basePosition = target.localPosition;
            baseRotation = target.localRotation;
            hasBasePose = true;
        }

        /// <summary>Starts a layer. Replace-blended profiles clear the stack first.</summary>
        public void Play(ShakeProfile profile, float scale = 1f)
        {
            if (profile == null || scale <= 0f)
            {
                return;
            }

            if (profile.Blend == ShakeBlend.Replace)
            {
                layers.Clear();
            }

            if (layers.Count >= maxLayers)
            {
                layers.RemoveAt(0);
            }

            layers.Add(new Layer
            {
                Profile = profile,
                Age = 0f,
                Scale = scale * globalScale,
                Seed = Random.Range(0, 997),
            });
        }

        public void Clear()
        {
            layers.Clear();
        }

        private void LateUpdate()
        {
            if (!hasBasePose)
            {
                return;
            }

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            ShakeOffset sum = default;
            int contributing = 0;

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                Layer layer = layers[i];
                layer.Age += deltaTime;

                if (layer.Age >= layer.Profile.Duration)
                {
                    layers.RemoveAt(i);
                    continue;
                }

                if (i == 0 && layers.Count > 0 && layers[0].Profile.Blend == ShakeBlend.Replace)
                {
                    // A replace layer owns the pose; additive layers underneath are muted.
                    sum = layer.Profile.Sample(layer.Age, layer.Seed) * layer.Scale;
                    contributing = 1;
                    continue;
                }

                sum += ShakeOffset.Scale(layer.Profile.Sample(layer.Age, layer.Seed), layer.Scale);
                contributing++;
            }

            if (contributing == 0)
            {
                target.localPosition = basePosition;
                target.localRotation = baseRotation;
                return;
            }

            ShakeOffset final = ShakeMath.Combine(sum, contributing);
            target.localPosition = basePosition + final.Position;
            target.localRotation = baseRotation * Quaternion.Euler(final.Euler);
        }
    }
}
