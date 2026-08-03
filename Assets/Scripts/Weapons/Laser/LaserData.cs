using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/LaserData", fileName = "LaserData")]
    public class LaserData : BulletData
    {
        public Color Color = Color.red;
        [Tooltip("The range at which the line will be drawn if no target exists. Has no effect on the actual range of the weapon.")]
        public float Range = 10;
        public float LineWidth = 0.1f;
        public float OverheatThreshold = 1;
        public float HeatLoss = .4f;
        public float HeatPerShot = .1f;
        public override float Speed => float.MaxValue;
    }
}