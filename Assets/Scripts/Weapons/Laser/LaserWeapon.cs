using UnityEngine;
using Utilities;
namespace Weapons
{
    public class LaserWeapon : WeaponBase
    {
        [SerializeField] protected float heatPerShot = .1f;
        [SerializeField] protected float overheatThreshold = 1.0f;
        [SerializeField] protected float heatLoss = .34f;
        protected bool overheated;
        protected float currentHeat;

        protected override void OnEnable()
        {
            base.OnEnable();
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformFixedUpdate);
        }

        protected void OnDisable()
        {
            var u = GlobalUpdater.TryGetInstance();
            if (u != null)
            {
                u.UnregisterFixedUpdate(PerformFixedUpdate);
            }
        }

        public void PerformFixedUpdate(float dt)
        {
            currentHeat = Mathf.Max(0, currentHeat - heatLoss * dt);
            if (currentHeat == 0) overheated = false;
        }

        public override void PerformReset()
        {
            base.PerformReset();
            overheated = false;
            currentHeat = 0;
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