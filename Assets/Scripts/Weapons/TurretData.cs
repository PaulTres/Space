using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(fileName = "TurretData", menuName = "Weapons/Turret/TurretData")]
    public class TurretData : ScriptableObject
    {
        #region Target selection
        [Header("Target selection")]
        public bool AllowZeroScoreTarget;
        public TargetEvaluation TargetEvaluation;
        public float MinScoreDelta;
        public bool TargetMissiles;
        public bool TargetUnits = true;
        public float EvaluateTarget(Turret shooter, IObject @object)
        {
            return TargetEvaluation != null ? TargetEvaluation.Evaluate(shooter, @object) : 0;
        }
        #endregion

        [Header("Shooting")]
        [Tooltip("The turret will only fire when the angle between its barrel(s) and the " +
            "target is <= this value.")]
        public float MaxAngleDifference = 1;
        public float MaxRange = 100;
        protected float maxRangeSquared = -1;
        public float MaxRangeSquared
        {
            get
            {
                if (maxRangeSquared < 0)
                {
                    maxRangeSquared = MaxRange * MaxRange;
                }
                return maxRangeSquared;
            }
        }
        [Header("Turret movement")]
        public float RotSpeed = 50;
        public float MinElevation = 0;
        public float MaxElevation = 90;
        public float MinRot = -190;
        public float MaxRot = 190;
    }
}