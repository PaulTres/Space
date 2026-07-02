using UnityEngine;
namespace Weapons
{
    /// <summary>
    /// Parameters of a weapon.
    /// </summary>
    [CreateAssetMenu(menuName = "Weapons/WeaponParameters", fileName = "WeaponParameters")]
    [System.Serializable]
    public class WeaponParameters : ScriptableObject
    {
        [Tooltip("cooldown between shots.")]
        public float MaxShotCooldown = 1, RateOfFireRampUp = 0.1f, RateOfFireRampDown = 0.2f,
            MinShotCooldown = 0.1f, SignatureIncrease = 1;
        public BulletData BulletData;
        protected virtual void OnValidate()
        {
            Debug.Assert(BulletData != null);
        }
        public Bullet GetInstance(Transform point) => BulletData.GetInstance(point);
    }
}