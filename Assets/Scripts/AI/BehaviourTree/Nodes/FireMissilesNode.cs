using HybridBT2;
using System;
using UnityEngine;
using Weapons;
[CreateAssetMenu(menuName = "HybridBT2/Leaves/FireMissiles", fileName = "FireMissiles")]
public class FireMissilesNode : LeafNodeData
{
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var launchers = ctx.GetData<MissileLauncher[]>(Blackboard.Keys.Launchers);
        if (launchers == null || launchers.Length == 0) return NodeState.FAILURE;
        var target = ctx.GetData<Unit>(Blackboard.Keys.Target);
        if (target == null) return NodeState.FAILURE;
        if (!target.gameObject.activeSelf)
        {
            ctx.SetData<Unit>(Blackboard.Keys.Target, null);
            return NodeState.FAILURE;
        }

        for (int i = 0; i < launchers.Length; i++)
        {
            launchers[i].Shoot(target);
        }

        return NodeState.RUNNING;
    };
}