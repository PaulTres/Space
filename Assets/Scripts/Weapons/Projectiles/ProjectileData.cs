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
        [SerializeField] protected bool resetCurve = true;
        [SerializeField] protected bool validateData = true;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (resetCurve)
            {
                resetCurve = false;
                TrailRendererCurve = new AnimationCurve(
                    new Keyframe(0f, ScaleFactor),
                    new Keyframe(.35f, 0f)
                );
                Debug.Log("Resetting curve on " + this + ".");
            }
            if (validateData)
            {
                validateData = false;
                TopSpeed = Mathf.Max(TopSpeed, 2 * InitialVelocity);
                Debug.Log("Validating data for " + this + '.');
            }
        }

        protected virtual void Reset()
        {
            TrailRendererCurve = new AnimationCurve(
                new Keyframe(0f, ScaleFactor),
                new Keyframe(.35f, 0f)
            );
        }
    }
}