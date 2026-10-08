using UnityEngine;
namespace Weapons
{
    public class LaserWeapon : WeaponBase
    {
        protected override void PerformBulletInitialization(Bullet bullet, ImprovedMonoBehaviour target)
        {
            if (target != null) ((Laser)bullet).SetTarget(target);
            else Debug.LogWarning($"Laser {transform} has no target!");
        }
    }
}