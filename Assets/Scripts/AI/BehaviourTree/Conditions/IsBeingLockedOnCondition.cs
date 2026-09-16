using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(fileName = "IsBeingLockedOnCondition", menuName = "HybridBT2/Conditions/IsBeingLockedOnCondition")]
public class IsBeingLockedOnCondition : ConditionData
{
    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        return target.Active() && target.InOptimalFiringRange(ctx.Navigation);
    };
}
