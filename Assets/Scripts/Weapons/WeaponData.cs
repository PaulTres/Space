using UnityEngine;
namespace Weapons
{
    /// <summary>
    /// Parameters of a Weapon.
    /// </summary>
    [CreateAssetMenu(menuName = "Weapons/WeaponData", fileName = "WeaponData")]
    [System.Serializable]
    public class WeaponData : ScriptableObject
    {
        [Tooltip("Maximum cooldown between shots. The shot cooldown starts at this value.")]
        public float MaxShotCooldown = 5;
        [Tooltip("How quickly the rate of fire of this Weapon increases when continuously firing.")]
        public float RateOfFireRampUp = 0.1f;
        [Tooltip("How quickly the rate of fire of this Weapon decreases when not continuously firing.")]
        public float RateOfFireRampDown = 1f;
        [Tooltip("Minimum shot cooldown.")]
        public float MinShotCooldown = 1;
        [Tooltip("The amount by which the owner's signature will increase with every shot.")]
        public float SignatureIncrease = 1;
        [Tooltip("The bullet this Weapon will fire.")]
        public BulletData BulletData;

        protected virtual void OnValidate()
        {
            Debug.Assert(BulletData != null);
        }

        public Bullet GetInstance(Transform point) => BulletData.GetInstance(point);
    }
}