using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/LaserData", fileName = "LaserData")]
    public class LaserData : BulletData
    {
        public Material LaserMaterial;
        [Tooltip("The range at which the line will be drawn if no target exists. Has no effect on the actual range of the weapon.")]
        public float Range = 10;
        public float LineWidth = 0.1f;
        public float HeatPerShot = .1f;
        public override float Speed => float.MaxValue;
    }
}