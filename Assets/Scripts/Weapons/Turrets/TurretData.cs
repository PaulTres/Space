using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(fileName = "TurretData", menuName = "Weapons/Turret/TurretData")]
    public class TurretData : ScriptableObject
    {
        #region Target selection
        [Header("Target selection")]
        public bool AllowZeroScoreTargets;
        public TargetEvaluation TargetEvaluation;
        public float MinScoreDelta;
        public bool TargetMissiles;
        public bool TargetUnits = true;
        [Tooltip("The score of the current target of the ship will be multiplied by this amount when evaluating targets(divided if negative).")]
        public float CurrentTargetScoreMultiplier = 2;
        public bool AccountForObstructions = true;

        public float EvaluateTarget(Turret shooter, ImprovedMonoBehaviour @object)
        {
            return TargetEvaluation != null ? TargetEvaluation.Evaluate(shooter, @object) : 0;
        }
        #endregion

        [Header("Shooting")]
        [Tooltip("The turret will only fire when the angle between its barrel(s) and the target is <= this value. Ignored if >= 360.")]
        public float MaxAngleDifference = 1;
        public float MaxRange = 100;
        public float MaxRangeSquared => MaxRange * MaxRange;
        [Header("Burst fire")]
        public bool BurstFire = false;
        public float BurstCooldown = 1;
        public int BurstCount = 1;
        [Header("Turret movement")]
        public float RotSpeed = 50;

        protected void OnValidate()
        {
            Debug.Assert(RotSpeed >= 0, $"Turret rotation speed must be >= 0 on {this}.");
            if (name.Contains("PD") && !TargetMissiles) Debug.LogWarning("TurretData is named PD but TargetMissiles is false. This may be a mistake.");
            Debug.Assert(TargetEvaluation != null, $"TargetEvaluation must be assigned on {this}.");
        }
    }
}