using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Projectile Data", fileName = "Projectile Data")]
    public class ProjectileData : BulletData
    {
        public float TopSpeed, Acceleration, InitialVelocity;
        public bool InertialDampeners = true;
        public Vector3 Scale = Vector3.one;
        public Material RendererMaterial, TrailRendererMaterial;
        public AnimationCurve TrailRendererCurve;
        public override float Speed => (Mathf.Max(InitialVelocity, Acceleration) + TopSpeed) / 2;

        protected virtual void Reset()
        {
            TrailRendererCurve = new AnimationCurve(
                new Keyframe(0f, Scale.y),
                new Keyframe(Scale.y, 0f)
            );
        }
    }
}