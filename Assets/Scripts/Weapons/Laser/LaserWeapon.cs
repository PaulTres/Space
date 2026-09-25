using UnityEngine;
namespace Weapons
{
    public class LaserWeapon : WeaponBase
    {
        protected float heatPerShot = .1f;
        [field: SerializeField] public Heatsink Heatsink { get; protected set; }

        protected override void OnValidate()
        {
            base.OnValidate();
            //Debug.Log("A");
            Transform parent = transform;
            while (parent != null)
            {
                Heatsink = parent.GetComponent<Heatsink>();
                if (Heatsink != null) break;
                parent = parent.parent;
            }
            if (transform.root.GetComponent<Unit>() != null) Debug.Assert(Heatsink != null, $"{this} on {transform.root} has no Heatsink for lasers!");
        }

        protected override void Awake()
        {
            base.Awake();
            var data = @params as LaserWeaponData;
            if (data == null) throw new System.Exception($"Weapons {name} has no LaserWeaponData!");
            heatPerShot = data.HeatPerShot;
            Debug.Assert(Heatsink != null, $"{this} on {transform.root} has no Heatsink for lasers!");
        }

        public override bool CanShoot()
        {
            return base.CanShoot() && !Heatsink.Overheated;
        }

        protected override void PerformBulletInitialization(Bullet bullet, ImprovedMonoBehaviour target)
        {
            if (target != null) ((Laser)bullet).SetTarget(target);
            else Debug.LogWarning($"Laser {transform} has no target!");
            Heatsink.IncreaseHeat(heatPerShot);
        }
    }
}