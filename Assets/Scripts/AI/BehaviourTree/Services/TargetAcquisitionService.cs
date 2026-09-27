using HybridBT2;
using System;
using UnityEngine;
using Utilities;
using Weapons;

[CreateAssetMenu(menuName = "HybridBT2/Services/TargetAcquisition", fileName = "TargetAcquisition")]
public class TargetAcquisitionService : ServiceData
{
    [Tooltip("The minimum difference in utility score between the best target and the next " +
        "best target for the best target to be selected.")]
    public float MinUtilityDelta = 0;
    public float MaxRange = 1000;
    public TargetEvaluation TargetEvaluation;
    [Tooltip("The score of the target of the escort candidate will be multiplied by this amount.")]
    //[SerializeField] protected float escortCandidateTargetMultiplier = 2;

    public override Action<Node, Blackboard> Action => (n, ctx) =>
    {
        ImprovedMonoBehaviour bestTarget = null;
        float bestTargetScore = float.MinValue;
        //var escortCandidate = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.EscortCandidate);
        //if (escortCandidate.Active())
        //{
        //    bestTarget = escortCandidate.Target;
        //    if (bestTarget.Active()) bestTargetScore = escortCandidateTargetMultiplier * TargetEvaluation.Evaluate(ctx.Navigation, bestTarget, MaxRange);
        //}
        foreach (var target in ctx.Navigation.GetTargets())
        {
            if (!target.Active() || target.Untargetable) continue;
            if (!bestTarget.Active())
            {
                bestTarget = target;
                bestTargetScore = TargetEvaluation.Evaluate(ctx.Navigation, target, MaxRange);
            }
            else
            {
                var score = TargetEvaluation.Evaluate(ctx.Navigation, target, MaxRange);
                if (score > bestTargetScore + MinUtilityDelta)
                {
                    bestTarget = target;
                    bestTargetScore = score;
                }
            }
        }
        //Debugging.Log($"TargetAcquisitionService: bestTarget={bestTarget}, bestTargetScore={bestTargetScore}");
        ctx.SetData(Blackboard.Keys.Target, bestTarget);
    };
}
