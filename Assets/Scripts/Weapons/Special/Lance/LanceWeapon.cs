using UnityEngine;

namespace Weapons
{
    public class LanceWeapon : WeaponBase
    {
        public override void ProcessBullet(Bullet bullet, ImprovedMonoBehaviour target, Transform shootPoint)
        {
            var lance = (LanceBullet)bullet;
            lance.ShootPoint = shootPoint;
            lance.SetTarget(target);
            base.ProcessBullet(bullet, target, shootPoint);
        }
    }
}