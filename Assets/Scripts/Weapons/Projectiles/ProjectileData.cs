using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Projectile Data", fileName = "Projectile Data")]
    public class ProjectileData : BulletData
    {
        public float TopSpeed, Acceleration, InitialVelocity;
        public bool InertialDampeners = true;
        public Vector3 Scale = Vector3.one;
        public Material RendererMaterial;
    }
}