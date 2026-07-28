using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(fileName = "TargetInRange", menuName = "HybridBT2/Conditions/TargetInRange")]
public class TargetInRangeCondition : ConditionData
{
    [SerializeField] protected float Range = 50;
    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        var target = ctx.GetData<Unit>(Blackboard.Keys.Target);
        if (target.Active())
        {
            return Vector3.Distance(target.Position, ctx.Position) <= Range;
        }
        return false;
    };
}
