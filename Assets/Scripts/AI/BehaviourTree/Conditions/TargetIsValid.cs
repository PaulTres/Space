using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(menuName = "HybridBT2/Conditions/TargetIsValid", fileName = "TargetIsValid")]
public class TargetIsValid : ConditionData
{
    public override Func<Node, Blackboard, bool> Function => (n, b) =>
    {
        return b.GetData<Unit>(Blackboard.Keys.Target).Active();
    };
}
