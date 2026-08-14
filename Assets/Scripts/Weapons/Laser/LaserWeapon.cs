using UnityEngine;
namespace Weapons
{
    public class LaserWeapon : WeaponBase
    {
        protected float heatPerShot = .1f;
        protected Heatsink heatsink;

        protected override void Awake()
        {
            base.Awake();
            var data = @params as LaserWeaponData;
            if (data == null) throw new System.Exception($"Weapons {name} has no LaserWeaponData!");
            heatPerShot = data.HeatPerShot;
            heatsink = transform.root.GetComponentInChildren<Heatsink>();
            Debug.Assert(heatsink != null, $"No heatsink on {transform.root}!");
        }

        public override bool CanShoot()
        {
            return base.CanShoot() && !heatsink.Overheated;
        }

        public override void ProcessBullet(Bullet bullet, ImprovedMonoBehaviour target, Transform shootPoint)
        {
            if (target != null) ((Laser)bullet).SetTarget(target);
            else Debug.LogWarning($"Laser {transform} has no target!");
            heatsink.IncreaseHeat(heatPerShot);
            base.ProcessBullet(bullet, target, shootPoint);
        }
    }
}