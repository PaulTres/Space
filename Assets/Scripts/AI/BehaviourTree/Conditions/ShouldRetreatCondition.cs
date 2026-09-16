using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(fileName = "ShouldRetreatCondition", menuName = "HybridBT2/Conditions/ShouldRetreatCondition")]
public class ShouldRetreatCondition : ConditionData
{
    [SerializeField] protected float distanceBuffer = 10;

    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        return target.Active() &&
            //target.Target.IsEqual(ctx.Navigation) && 
            Vector3.Distance(target.Position, ctx.Position) <= target.ScanRange + ctx.Navigation.CurrentSignature + distanceBuffer;
    };
}
