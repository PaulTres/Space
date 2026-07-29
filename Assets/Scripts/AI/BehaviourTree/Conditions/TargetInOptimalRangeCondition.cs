using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(fileName = "TargetInRange", menuName = "HybridBT2/Conditions/TargetInRange")]
public class TargetInOptimalRangeCondition : ConditionData
{
    [SerializeField] protected float minRangeFromTarget = -1;
    [Tooltip("The condition returns true if the distance between the user and the " +
        "target is <= (user.ScanRange+Target.Signature+buffer).")]
    [SerializeField] protected float buffer = 0;
    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        var target = ctx.GetData<Unit>(Blackboard.Keys.Target);
        if (target.Active())
        {
            var dist = Vector3.Distance(target.Position, ctx.Position);
            return minRangeFromTarget < dist && dist <= target.Signature + ctx.Unit.ScanRange + buffer;
        }
        return false;
    };
}
