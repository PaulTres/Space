using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/LaserWeaponData", fileName = "LaserWeaponData")]
    [System.Serializable]
    public class LaserWeaponData : WeaponData
    {
        public float HeatPerShot = .1f;
        public float OverheatThreshold = 1.0f;
        public float HeatLoss = .34f;
        [Tooltip("The weapon will not lose heat until at least this much time has passed after firing last time.")]
        public float HeatLossCooldown = .5f;
    }
}