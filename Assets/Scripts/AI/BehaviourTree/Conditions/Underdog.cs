using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(fileName = "Underdog", menuName = "HybridBT2/Conditions/Underdog")]
public class Underdog : ConditionData
{
    [SerializeField] protected float ownWeightModifier=1.25f;

    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        return target.Active() && target.MoraleWeight > ctx.Navigation.MoraleWeight * ownWeightModifier;
    };
}
