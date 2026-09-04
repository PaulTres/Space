using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Projectile Data", fileName = "Projectile Data")]
    public class ProjectileData : BulletData
    {
        public float TopSpeed = 20, Acceleration = 1, InitialVelocity = 10;
        public bool InertialDampeners = true;
        public float ScaleFactor = .1f;
        public Material RendererMaterial, TrailRendererMaterial;
        public AnimationCurve TrailRendererCurve;
        public override float Speed => (Mathf.Max(InitialVelocity, Acceleration) + TopSpeed) / 2;

        protected virtual void Reset()
        {
            TrailRendererCurve = new AnimationCurve(
                new Keyframe(0f, ScaleFactor),
                new Keyframe(1, 0f)
            );
        }
    }
}