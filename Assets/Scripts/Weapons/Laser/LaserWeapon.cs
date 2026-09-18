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
            heatsink = transform.GetComponentInParent<Heatsink>();
            Debug.Assert(heatsink != null, $"{transform.root} has no heatsink for lasers!");
            //if (transform.root.name.Contains("Shadow")) Debug.Log(currentShotCooldown);
        }

        public override bool CanShoot()
        {
            return base.CanShoot() && !heatsink.Overheated;
        }

        protected override void PerformBulletInitialization(Bullet bullet, ImprovedMonoBehaviour target)
        {
            if (target != null) ((Laser)bullet).SetTarget(target);
            else Debug.LogWarning($"Laser {transform} has no target!");
            heatsink.IncreaseHeat(heatPerShot);
        }
    }
}