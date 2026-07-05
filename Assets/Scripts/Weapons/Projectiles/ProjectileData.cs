using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Projectile Data", fileName = "Projectile Data")]
    public class ProjectileData : BulletData
    {
        public float TopSpeed, Acceleration, InitialVelocity;
    }
}