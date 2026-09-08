using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(fileName = "TargetInRange", menuName = "HybridBT2/Conditions/TargetInRange")]
public class TargetInOptimalRangeCondition : ConditionData
{
    [SerializeField] protected float minRangeFromTarget = -1;
    [Tooltip("The condition returns true if the distance between the user and the " +
        "target is <= (user.ScanRange+Target.CurrentSignature+buffer).")]
    [SerializeField] protected float buffer = 0;
    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        if (target.Active())
        {
            //Debug.Log($"TargetInOptimalRangeCondition: target={target}, minRangeFromTarget={minRangeFromTarget}, maxRange={target.CurrentSignature + ctx.Navigation.ScanRange + buffer}");
            var dist = Vector3.Distance(target.Position, ctx.Position);
            if (target == GameManager.Player && Name.Contains("Medium"))
            {
                //Debug.Log($"TargetInOptimalRangeCondition: dist={dist}, minRangeFromTarget={minRangeFromTarget}, maxRange={target.CurrentSignature + ctx.Navigation.ScanRange + buffer}");
            }
            //if (Name.Contains("Medium")) Debug.Log($"TargetInOptimalRangeCondition: dist={dist}, minRangeFromTarget={minRangeFromTarget}, maxRange={target.CurrentSignature + ctx.Navigation.ScanRange + buffer}");
            return minRangeFromTarget <= dist && dist <= target.CurrentSignature + ctx.Navigation.ScanRange + buffer;
        }
        return false;
    };
}
