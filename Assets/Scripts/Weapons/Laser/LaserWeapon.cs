using UnityEngine;
namespace Weapons
{
    public class LaserWeapon : WeaponBase
    {
        public override void ProcessBullet(Bullet bullet, IObject target)
        {
            if (target != null) ((Laser)bullet).Target = target.Transform;
            else Debug.LogWarning($"{transform} has no target!");
                base.ProcessBullet(bullet, target);
        }
    }
}