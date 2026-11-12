using UnityEngine;

namespace ShakeKit
{
    /// <summary>One sampled displacement for a single frame.</summary>
    public struct ShakeOffset
    {
        public Vector3 Position;
        public Vector3 Euler;

        public static ShakeOffset operator +(ShakeOffset a, ShakeOffset b) => new ShakeOffset
        {
            Position = a.Position + b.Position,
            Euler = a.Euler + b.Euler,
        };

        public static ShakeOffset Scale(ShakeOffset value, float factor) => new ShakeOffset
        {
            Position = value.Position * factor,
            Euler = value.Euler * factor,
        };
    }

    /// <summary>Noise sampling and layer mixing used by <see cref="CameraShaker"/>.</summary>
    public static class ShakeMath
    {
        /// <summary>
        /// Continuous Perlin channels, decorrelated per axis by seed offsets.
        /// Sampling on a continuous time axis keeps fast shakes from strobing.
        /// </summary>
        public static ShakeOffset Sample(float time, float frequency, float positionAmplitude,
            float rotationAmplitude, int seed)
        {
            float t = time * frequency;
            float sx = seed * 0.137f;
            float sy = seed * 0.271f + 11.3f;
            float sz = seed * 0.419f + 23.7f;

            return new ShakeOffset
            {
                Position = new Vector3(
                    Unit(sx, t) * positionAmplitude,
                    Unit(sy, t) * positionAmplitude,
                    Unit(sz, t) * positionAmplitude * 0.35f),
                Euler = new Vector3(
                    Unit(sz, t * 1.13f) * rotationAmplitude,
                    Unit(sx, t * 0.87f) * rotationAmplitude,
                    Unit(sy, t * 1.31f) * rotationAmplitude * 0.5f),
            };
        }

        /// <summary>Signed Perlin value in [-1, 1] on two decorrelated axes.</summary>
        public static float Unit(float channel, float t) =>
            Mathf.PerlinNoise(channel, t) * 2f - 1f;

        /// <summary>
        /// Additive layers sum, but are normalized by sqrt(count) so stacking
        /// four small shakes feels roughly as loud as one large shake.
        /// </summary>
        public static ShakeOffset Combine(ShakeOffset accumulated, int layers)
        {
            if (layers <= 1)
            {
                return accumulated;
            }

            return Scale(accumulated, 1f / Mathf.Sqrt(layers));
        }
    }
}
