using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/LaserWeaponData", fileName = "LaserWeaponData")]
    [System.Serializable]
    public class LaserWeaponData : WeaponData
    {
        public float HeatPerShot = .1f;
    }
}