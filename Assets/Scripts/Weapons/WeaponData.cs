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
        [Tooltip("The amount by which the owner's signature will increase with every shot.")]
        public float SignatureIncrease = 1;
        [Tooltip("The bullet this Weapons will fire.")]
        public BulletData BulletData;
        [Tooltip("If true, this weapon will only fire when the target is in optimal firing range of the shooter.")]
        public bool RequiresOptimalFiringRange = true;
        [Tooltip("If true, the weapon will be ready to fire the moment the owner spawns.")]
        public bool StartLoaded = true;

        protected virtual void OnValidate()
        {
            Debug.Assert(BulletData != null, $"{this} has no bullet data!");
        }

        public Bullet GetInstance(Transform point) => BulletData.GetInstance(point);
    }
}