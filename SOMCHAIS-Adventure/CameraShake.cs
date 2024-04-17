using System;
using Microsoft.Xna.Framework;
using SOMCHAIS_Adventure;

namespace SOMCHAISAdventure
{
    class CameraShake
    {
        public bool isShakingEnabled;
        public bool isShaking;
        private float shakeTime;
        private float shakeDuration;
        private Vector2 shakeAmount;
        private readonly Random random;
        private float elapsedTimeSinceLastShake;

        public CameraShake(float shakeTime, Vector2 shakeAmount)
        {
            this.shakeTime = shakeTime;
            this.shakeAmount = shakeAmount;
            random = new Random();
        }

        public void Update(GameTime gameTime, ref Vector2 cameraPosition)
        {
            // Increment the elapsed time since the last shake
            elapsedTimeSinceLastShake += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check if it's time to trigger a new shake
            if (elapsedTimeSinceLastShake >= shakeTime * 1.5f)
            {
                isShakingEnabled = true;
                isShaking = true;
                shakeDuration = 0f;
                elapsedTimeSinceLastShake = 0f;
            }

            if (isShaking)
            {
                shakeDuration += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (shakeDuration >= shakeTime)
                {
                    isShaking = false;
                    shakeDuration = 0f;
                }
                else
                {
                    float t = shakeDuration / shakeTime;
                    float damper = 1.0f - SmoothStep(0, 1, t);
                    Vector2 offset = new Vector2(
                        (float)random.NextDouble() * shakeAmount.X * damper,
                        (float)random.NextDouble() * shakeAmount.Y * damper
                    );

                    cameraPosition += offset;
                }
            }
        }

        public void ToggleShake(GameTime gameTime)
        {
            isShakingEnabled = !isShakingEnabled;
            if (isShakingEnabled)
            {
                isShaking = true;
            }
        }

        private static float SmoothStep(float edge0, float edge1, float x)
        {
            x = Clamp((x - edge0) / (edge1 - edge0), 0.0f, 1.0f);
            return x * x * (3.0f - 2.0f * x);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    }
}