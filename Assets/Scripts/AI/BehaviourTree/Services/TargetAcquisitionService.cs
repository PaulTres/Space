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

    public override Action<Node, Blackboard> Action => (n, ctx) =>
    {
        Unit bestTarget = null;
        float bestTargetScore = float.MinValue;
        foreach (var target in ctx.Navigation.GetTargets())
        {
            if (!target.Active()) continue;
            if (bestTarget == null)
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
        //Debug.Log($"TargetAcquisitionService: bestTarget={bestTarget}, bestTargetScore={bestTargetScore}");
        ctx.SetData(Blackboard.Keys.Target, bestTarget);
    };
}
