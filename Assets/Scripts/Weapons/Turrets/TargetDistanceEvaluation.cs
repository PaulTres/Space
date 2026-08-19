using UnityEngine;
using Utilities;
namespace Weapons
{
    [CreateAssetMenu(fileName = "ClosestTargetEvaluation", menuName = "Weapons/Turret/ClosestTargetEvaluation")]
    public class TargetDistanceEvaluation : TargetEvaluation
    {
        public override float Evaluate(Unit shooter, ImprovedMonoBehaviour target, float maxRange)
        {
            if (!target.Active())
            {
                Debug.LogError($"{this} received invalid target from {shooter}.");
                return float.MinValue;
            }
            var dir = target.Transform.position - shooter.Transform.position;
            return Mathf.Max(maxRange - dir.sqrMagnitude, 0) / maxRange;
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