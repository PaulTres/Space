using UnityEngine;

namespace Weapons
{
    public class MissileLauncher : WeaponBase
    {
        public override void ProcessBullet(Bullet bullet, IObject target)
        {
            if (target != null) ((Missile)bullet).SetTarget(target);
            else Debug.LogWarning($"Missile launcher {transform} was not passed a target!");
            base.ProcessBullet(bullet, target);
        }
    }
}