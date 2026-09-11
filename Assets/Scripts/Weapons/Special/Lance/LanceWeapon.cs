namespace Weapons
{
    public class LanceWeapon : WeaponBase
    {
        protected override void PerformBulletInitialization(Bullet bullet, ImprovedMonoBehaviour target)
        {
            ((LanceBullet)bullet).SetTarget(target);
        }
    }
}