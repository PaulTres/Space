using UnityEngine;
using Utilities;
namespace Weapons
{
    [CreateAssetMenu(fileName = "ClosestTargetEvaluation", menuName = "Weapons/Turret/ClosestTargetEvaluation")]
    public class TargetDistanceEvaluation : TargetEvaluation
    {
        [Tooltip("This multiplier is applied to enemies targeting this unit.")]
        [SerializeField] protected float targetingMeMultiplier = 2;

        public override float Evaluate(Unit shooter, ImprovedMonoBehaviour target, float maxRange)
        {
            if (!target.Active())
            {
                Debug.LogError($"{this} received invalid target from {shooter}.");
                return float.MinValue;
            }
            var dir = target.Transform.position - shooter.Transform.position;
            var score = Mathf.Max(maxRange - dir.magnitude, 0) / maxRange;
            if (target.Target == shooter) score *= targetingMeMultiplier;
            return score;
        }

        public override float Evaluate(Turret shooter, ImprovedMonoBehaviour target)
        {
            if (!target.Active())
            {
                Debug.LogError($"{this} received invalid target from {shooter}.");
                return float.MinValue;
            }
            var dir = target.Transform.position - shooter.Unit.Transform.position;
            return Mathf.Max(shooter.MaxRangeSquared - dir.sqrMagnitude, 0) / shooter.MaxRangeSquared;
        }
    }
}