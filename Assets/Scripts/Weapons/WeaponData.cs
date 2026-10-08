using UnityEngine;
namespace Weapons
{
    /// <summary>
    /// Parameters of a Weapons.
    /// </summary>
    [CreateAssetMenu(menuName = "Weapons/WeaponData", fileName = "WeaponData")]
    [System.Serializable]
    public class WeaponData : ScriptableObject
    {
        [Tooltip("Maximum cooldown between shots. The shot cooldown starts at this value.")]
        public float MaxShotCooldown = 5;
        [Tooltip("How quickly the rate of fire of this Weapons increases when continuously firing.")]
        public float RateOfFireRampUp = 0.1f;
        [Tooltip("How quickly the rate of fire of this Weapons decreases when not continuously firing.")]
        public float RateOfFireRampDown = 1f;
        [Tooltip("Minimum shot cooldown.")]
        public float MinShotCooldown = 1;
        [SerializeField]
        protected AnimationCurve readinessCurve = new(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f)
            );
        [Tooltip("Editor only. Will reset the value of the curve to a downward slope.")]
        [SerializeField] protected bool resetCurve;
        [Tooltip("The amount by which the owner's signature will increase with every shot.")]
        public float SignatureIncrease = 1;
        [Tooltip("The bullet this weapon will fire.")]
        public BulletData BulletData;
        [Tooltip("If true, this weapon will only fire when the target is in optimal firing range of the shooter.")]
        public bool RequiresOptimalFiringRange = true;
        [Tooltip("If true, the weapon will be ready to fire the moment the owner spawns.")]
        public bool StartLoaded = true;
        public float HeatPerShot = .1f;

        protected virtual void OnValidate()
        {
            Debug.Assert(BulletData != null, $"{this} has no bullet data!");
            readinessCurve ??= new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f)
            );
            if (resetCurve)
            {
                resetCurve = false;
                readinessCurve = new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(1f, 0f)
                );
            }
            MinShotCooldown = Mathf.Min(MinShotCooldown, MaxShotCooldown);
        }

        public float GetCurveValue(float value) => Mathf.Clamp01(readinessCurve.Evaluate(value));

        public Bullet GetInstance(Transform point) => BulletData.GetInstance(point);
    }
}