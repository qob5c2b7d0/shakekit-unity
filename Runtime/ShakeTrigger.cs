using UnityEngine;

namespace ShakeKit
{
    /// <summary>Demo driver: Space fires a random profile, C clears all layers.</summary>
    [RequireComponent(typeof(CameraShaker))]
    public sealed class ShakeTrigger : MonoBehaviour
    {
        [SerializeField] private ShakeProfile[] profiles;
        [SerializeField, Range(0.1f, 2f)] private float scale = 1f;

        private CameraShaker shaker;

        private void Awake()
        {
            shaker = GetComponent<CameraShaker>();
        }

        private void Update()
        {
            if (profiles == null || profiles.Length == 0)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                shaker.Play(profiles[Random.Range(0, profiles.Length)], scale);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                shaker.Clear();
            }
        }
    }
}
