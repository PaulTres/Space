using UnityEngine;

namespace Weapons
{
    public class LanceWeapon : WeaponBase
    {
        public override void ProcessBullet(Bullet bullet, ImprovedMonoBehaviour target, Transform shootPoint)
        {
            //Debug.Log((LanceBullet)bullet);
            ((LanceBullet)bullet).SetTarget(target);
            base.ProcessBullet(bullet, target, shootPoint);
        }
    }
}