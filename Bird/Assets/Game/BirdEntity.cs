// Copyright CodeGamified 2025-2026
// MIT License — Flappy Bird
using UnityEngine;
using CodeGamified.Time;

namespace Bird.Game
{
    /// <summary>
    /// The bird entity — position, velocity, gravity, flap.
    /// Continuous physics, time-scale aware.
    /// Y increases upward. Ground at Y=0, ceiling at Y=WorldHeight.
    /// </summary>
    public class BirdEntity : MonoBehaviour
    {
        // Config
        public float Gravity { get; set; } = -25f;
        public float FlapStrength { get; set; } = 8f;
        public float MaxFallSpeed { get; set; } = -15f;
        public float WorldHeight { get; set; } = 10f;

        // State
        public float PosY { get; private set; }
        public float VelocityY { get; private set; }
        public bool IsDead { get; private set; }

        // Events
        public System.Action OnFlapped;
        public System.Action OnDied;

        public void Initialize(float startY, float gravity, float flapStrength,
                               float maxFallSpeed, float worldHeight)
        {
            Gravity = gravity;
            FlapStrength = flapStrength;
            MaxFallSpeed = maxFallSpeed;
            WorldHeight = worldHeight;
            Reset(startY);
        }

        public void Reset(float startY)
        {
            PosY = startY;
            VelocityY = 0f;
            IsDead = false;
            SyncTransform();
        }

        private void Update()
        {
            if (IsDead) return;
            if (SimulationTime.Instance == null || SimulationTime.Instance.isPaused) return;

            float dt = Time.deltaTime * (SimulationTime.Instance?.timeScale ?? 1f);
            StepPhysics(dt);
            SyncTransform();
        }

        private void StepPhysics(float dt)
        {
            // Sub-step for high time scales to prevent tunneling
            int steps = Mathf.Max(1, Mathf.CeilToInt(dt / 0.005f));
            float subDt = dt / steps;

            for (int i = 0; i < steps && !IsDead; i++)
            {
                VelocityY += Gravity * subDt;
                VelocityY = Mathf.Max(VelocityY, MaxFallSpeed);
                PosY += VelocityY * subDt;

                // Ground / ceiling collision
                if (PosY <= 0f)
                {
                    PosY = 0f;
                    VelocityY = 0f;
                    Die();
                    return;
                }
                if (PosY >= WorldHeight)
                {
                    PosY = WorldHeight;
                    VelocityY = 0f;
                    Die();
                    return;
                }
            }
        }

        /// <summary>Flap — apply upward impulse. Called by player code.</summary>
        public void Flap()
        {
            if (IsDead) return;
            VelocityY = FlapStrength;
            OnFlapped?.Invoke();
        }

        public void Die()
        {
            if (IsDead) return;
            IsDead = true;
            OnDied?.Invoke();
        }

        public void Stop()
        {
            VelocityY = 0f;
        }

        private void SyncTransform()
        {
            transform.position = new Vector3(0f, PosY, 0f);
        }
    }
}
