using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(fileName = "ClosestTargetEvaluation", menuName = "Weapons/Turret/ClosestTargetEvaluation")]
    public class TargetDistanceEvaluation : TargetEvaluation
    {
        public override float Evaluate(Unit shooter, ImprovedMonoBehaviour target, float maxRange)
        {
            var dir = target.Transform.position - shooter.Transform.position;
            return Mathf.Max(maxRange - dir.sqrMagnitude, 0) / maxRange;
        }

        public override float Evaluate(Turret shooter, ImprovedMonoBehaviour target)
        {
            //if (shooter.UnitProperty == GameManager.Player && !shooter.TargetMissiles && shooter.UnitProperty.Target.Active())
            //{
            //    Debug.Log(shooter.MaxRangeSquared);
            //}
            var dir = target.Transform.position - shooter.Unit.Transform.position;
            return Mathf.Max(shooter.MaxRangeSquared - dir.sqrMagnitude, 0) / shooter.MaxRangeSquared;
        }
    }
}