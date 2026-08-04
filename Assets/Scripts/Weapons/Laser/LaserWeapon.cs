using UnityEngine;
using Utilities;
namespace Weapons
{
    public class LaserWeapon : WeaponBase
    {
        protected float heatPerShot = .1f;
        protected float overheatThreshold = 1.0f;
        protected float heatLoss = .34f;
        protected float heatLossCooldown = .5f;
        protected bool overheated;
        protected float currentHeat;
        protected float lastShotTime;

        protected override void Awake()
        {
            base.Awake();
            var data = @params as LaserWeaponData;
            if (data == null) throw new System.Exception($"Weapons {name} has no LaserWeaponData!");
            heatPerShot = data.HeatPerShot;
            overheatThreshold = data.OverheatThreshold;
            heatLoss = data.HeatLoss;
            heatLossCooldown = data.HeatLossCooldown;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformFixedUpdate);
        }

        protected void OnDisable()
        {
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterFixedUpdate(PerformFixedUpdate);
        }

        public override void PerformReset()
        {
            base.PerformReset();
            overheated = false;
            lastShotTime = float.MinValue;
            currentHeat = 0;
        }

        public void PerformFixedUpdate(float dt)
        {
            if (Time.time - lastShotTime >= heatLossCooldown)
            {
                currentHeat = Mathf.Max(0, currentHeat - heatLoss * dt);
                if (currentHeat == 0) overheated = false;
            }
        }

        public override bool CanShoot()
        {
            return base.CanShoot() && !overheated;
        }

        public override void ProcessBullet(Bullet bullet, IObject target)
        {
            if (target != null) ((Laser)bullet).Target = target.Transform;
            else Debug.LogWarning($"{transform} has no target!");
            currentHeat += heatPerShot;
            if (currentHeat > overheatThreshold) overheated = true;
            base.ProcessBullet(bullet, target);
        }
    }
}