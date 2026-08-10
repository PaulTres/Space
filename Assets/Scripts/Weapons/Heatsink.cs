using AudioSystem;
using UnityEngine;
using Utilities;
namespace Weapons
{
    public class Heatsink : MonoBehaviour, IResettable
    {
        [SerializeField] protected float overheatThreshold = 1.0f;
        [SerializeField] protected float heatLoss = .34f;
        [SerializeField] protected float heatLossCooldown = .5f;
        [SerializeField] protected SoundData overheatSound;
        public bool Overheated { get; protected set; }
        public float CurrentHeat { get; protected set; }
        public float OverheatPercentage
        {
            get
            {
                return Mathf.Clamp01(CurrentHeat / overheatThreshold);
            }
        }
        protected float lastShotTime;

        protected void OnEnable()
        {
            PerformReset();
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformFixedUpdate);
        }

        protected void OnDisable()
        {
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterFixedUpdate(PerformFixedUpdate);
        }

        public void PerformReset()
        {
            Overheated = false;
            lastShotTime = float.MinValue;
            CurrentHeat = 0;
        }

        public void PerformFixedUpdate(float dt)
        {
            if (Time.time - lastShotTime >= heatLossCooldown)
            {
                CurrentHeat = Mathf.Max(0, CurrentHeat - heatLoss * dt);
                if (CurrentHeat == 0) Overheated = false;
            }
        }

        public void IncreaseHeat(float heat)
        {
            CurrentHeat += heat;
            lastShotTime = Time.time;
            if (CurrentHeat >= overheatThreshold)
            {
                Overheated = true;
                if (overheatSound != null)
                {
                    overheatSound.Play(transform.position);
                }
            }
        }
    }
}