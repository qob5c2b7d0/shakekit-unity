using UnityEngine;

namespace ShakeKit
{
    /// <summary>
    /// Broadcasts a shake to every <see cref="CameraShaker"/> inside a radius,
    /// scaled by the authored distance falloff. Aimed at explosions, landings
    /// and other world-space events rather than direct gameplay calls.
    /// </summary>
    public sealed class ImpulseSource : MonoBehaviour
    {
        [SerializeField] private ShakeProfile profile;
        [SerializeField, Min(0.1f)] private float radius = 8f;
        [SerializeField, Min(0f)] private float strength = 1f;
        [SerializeField] private AnimationCurve distanceFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        public void Play() => Play(transform.position);

        public void Play(Vector3 worldPoint) =>
            ImpulseHub.Dispatch(profile, worldPoint, radius, strength, distanceFalloff);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.55f, 0.15f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

    /// <summary>Static fan-out used by <see cref="ImpulseSource"/>.</summary>
    public static class ImpulseHub
    {
        public static void Dispatch(ShakeProfile profile, Vector3 origin, float radius,
            float strength, AnimationCurve falloff)
        {
            if (profile == null || radius <= 0f || strength <= 0f)
            {
                return;
            }

            CameraShaker[] shakers = Object.FindObjectsByType<CameraShaker>(FindObjectsSortMode.None);

            foreach (CameraShaker shaker in shakers)
            {
                float distance = Vector3.Distance(origin, shaker.transform.position);
                if (distance > radius)
                {
                    continue;
                }

                float normalized = distance / radius;
                float curve = falloff != null && falloff.length > 0
                    ? falloff.Evaluate(normalized)
                    : 1f - normalized;

                float scale = strength * Mathf.Clamp01(curve);
                if (scale > 0f)
                {
                    shaker.Play(profile, scale);
                }
            }
        }
    }
}
