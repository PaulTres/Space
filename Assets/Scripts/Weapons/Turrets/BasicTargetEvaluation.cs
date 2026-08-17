using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(fileName = "TargetEvaluation", menuName = "Weapons/Turret/TargetEvaluation")]
    public class BasicTargetEvaluation : TargetEvaluation
    {
        public override float Evaluate(Unit shooter, ImprovedMonoBehaviour target, float maxRange)
        {
            var dir = target.Transform.position - shooter.Transform.position;
            return Mathf.Max(maxRange - dir.sqrMagnitude, 0) / maxRange;
        }

        public override float Evaluate(Turret shooter, ImprovedMonoBehaviour target)
        {
            var dir = target.Position - shooter.Unit.Position;
            var angleScore = Vector3.Dot(dir.normalized, shooter.ElevationPivot.forward);
            var distScore = Mathf.Max(shooter.MaxRangeSquared - dir.sqrMagnitude, 0) / shooter.MaxRangeSquared;
            return angleScore * distScore;
        }
    }
}