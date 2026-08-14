using UnityEngine;

namespace Weapons
{
    public class MissileLauncher : WeaponBase
    {
        [SerializeField] protected Magazine magazine;
        public override void ProcessBullet(Bullet bullet, ImprovedMonoBehaviour target, Transform shootPoint)
        {
            if (magazine != null)
            {
                if (magazine.AmmoCount <= 0) return;
                magazine.Consume();
            }
            if (target != null) ((Missile)bullet).SetTarget(target);
            else Debug.LogWarning($"Missile launcher {transform} was not passed a target!");
            base.ProcessBullet(bullet, target, shootPoint);
        }
    }
}