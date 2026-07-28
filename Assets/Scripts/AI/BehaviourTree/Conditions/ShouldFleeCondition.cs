using HybridBT2;
using System;
using UnityEngine;


[CreateAssetMenu(fileName = "ShouldFlee", menuName = "HybridBT2/Conditions/ShouldFlee")]
public class ShouldFleeCondition : ConditionData
{
    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        var isFleeing = ctx.GetData<bool>(Blackboard.Keys.Fleeing);
        if (isFleeing)
        {
            return ctx.GetData<float>(Blackboard.Keys.Morale) <=
                GlobalConfig.MoraleRecoverThreshold *
                ctx.GetData<float>(Blackboard.Keys.DefaultMorale);
        }
        else
        {
            return ctx.GetData<float>(Blackboard.Keys.Morale) <=
                GlobalConfig.MoraleFleeThreshold *
                ctx.GetData<float>(Blackboard.Keys.DefaultMorale);
        }
    };
}
