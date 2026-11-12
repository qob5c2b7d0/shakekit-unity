using UnityEngine;

namespace ShakeKit
{
    public enum ShakeBlend
    {
        Additive,
        Replace,
    }

    /// <summary>
    /// Authored shake: how long it lasts, how far it displaces, and the
    /// falloff curve that shapes it. Curves may go negative to overshoot.
    /// </summary>
    [CreateAssetMenu(menuName = "ShakeKit/Shake Profile", fileName = "NewShakeProfile")]
    public sealed class ShakeProfile : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float duration = 0.35f;
        [SerializeField, Min(0f)] private float positionAmplitude = 0.12f;
        [SerializeField, Min(0f)] private float rotationAmplitude = 2.4f;
        [SerializeField, Min(0.01f)] private float frequency = 16f;
        [SerializeField] private ShakeBlend blend = ShakeBlend.Additive;
        [SerializeField] private AnimationCurve falloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

        public float Duration => duration;
        public ShakeBlend Blend => blend;
        public AnimationCurve Falloff => falloff;

        /// <summary>Envelope at normalized age [0, 1]; clamped so overshoot stays authored-only.</summary>
        public float Envelope(float normalizedAge)
        {
            if (falloff == null || falloff.length == 0)
            {
                return 1f - normalizedAge;
            }

            return Mathf.Max(0f, falloff.Evaluate(normalizedAge));
        }

        public ShakeOffset Sample(float age, int seed)
        {
            float normalizedAge = Mathf.Clamp01(age / duration);
            float envelope = Envelope(normalizedAge);
            ShakeOffset raw = ShakeMath.Sample(age, frequency, positionAmplitude, rotationAmplitude, seed);
            return ShakeOffset.Scale(raw, envelope);
        }

        private void OnValidate()
        {
            duration = Mathf.Max(0.01f, duration);
            frequency = Mathf.Max(0.01f, frequency);
        }
    }
}
