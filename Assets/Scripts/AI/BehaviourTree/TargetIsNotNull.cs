using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(menuName ="HybridBT2/Conditions/TargetIsNotNull",fileName ="TargetIsNotNull")]
public class TargetIsNotNull : ConditionData
{
    public override Func<Node, Blackboard, bool> Function => (n, b) =>
    {
        return b.GetData<Unit>(Blackboard.Keys.Target) != null;
    };
}
