using UnityEngine;

namespace Weapons
{
    public class MissileLauncher : WeaponBase
    {
        [SerializeField] protected Magazine magazine;
        public Magazine Magazine => magazine;

        protected override void PerformBulletInitialization(Bullet bullet, ImprovedMonoBehaviour target)
        {
            if (magazine != null)
            {
                if (magazine.AmmoCount <= 0) return;
                magazine.Consume();
            }
            if (target != null) ((Missile)bullet).SetTarget(target);
            else Debug.LogWarning($"Missile launcher {transform} was not passed a target!");
        }
    }
}