using HybridBT2;
using System;
using UnityEngine;
using Utilities;
using Weapons;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/DefensivePosture", fileName = "DefensivePosture")]
public class DefensivePostureNode : LeafNodeData
{
    [SerializeField] protected float speedScoreCap = 5;
    [SerializeField] protected float signatureScoreModifier = 1;
    [Tooltip("If a turret's score is below this value it will be set to a defensive stance.")]
    [SerializeField] protected float aggresionScoreThreshold = 1;

    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        if (!target.Active()) return NodeState.FAILURE;
        if (target.Target != ctx.Navigation)
        {
            foreach (var turret in ctx.GetData<Turret[]>(Blackboard.Keys.AllTurrets))
            {
                turret.SetAggressiveStance();
            }
            return NodeState.RUNNING;
        }
        float distanceToTarget = Vector3.Distance(target.Position, ctx.Position);
        foreach (var turret in ctx.GetData<Turret[]>(Blackboard.Keys.AllTurrets))
        {
            if (turret == null || !turret.gameObject.activeSelf || turret.TargetMissiles || turret.SignatureIncrease == 0) continue;
            var turretTarget = turret.Target;
            var speedScore = Mathf.Max(turret.BulletSpeed / (turretTarget.Active() ? turretTarget.MaxVelocityMagnitude : target.MaxVelocityMagnitude), speedScoreCap);
            var dps = turret.DPS;
            var signatureScore = turret.SignatureIncrease;
            var turretScore = speedScore * dps;
            if (ctx.Navigation.CurrentSignature > ctx.Navigation.MaximumSignature * .9f)
            {
                turretScore = aggresionScoreThreshold * 2;
            }
            else
            {
                var signatureModifier = (ctx.Navigation.CurrentSignature - ctx.Navigation.MinimumSignature) / (ctx.Navigation.MaximumSignature - ctx.Navigation.MinimumSignature);
                turretScore /= (signatureScore * signatureModifier + Mathf.Epsilon);
            }

            if (turretScore < aggresionScoreThreshold)
            {
                turret.SetDefensiveStance();
            }
            else
            {
                turret.SetAggressiveStance();
            }
        }
        return NodeState.RUNNING;
    };
}
