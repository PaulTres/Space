using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(fileName = "TargetTooClose", menuName = "HybridBT2/Conditions/TargetTooClose")]
public class TargetTooCloseCondition : ConditionData
{
    [SerializeField] protected float minRangeFromTarget = 10;
    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        if (target.Active())
        {
            var dist = Vector3.Distance(target.Position, ctx.Position);
            //Debugging.Log($"TargetTooCloseCondition: Distance to target is {dist}, min range is {minRangeFromTarget}");
            return dist <= minRangeFromTarget;
        }
        return false;
    };
}
