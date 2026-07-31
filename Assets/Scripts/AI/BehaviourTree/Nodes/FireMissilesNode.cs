using HybridBT2;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;
[CreateAssetMenu(menuName = "HybridBT2/Leaves/FireMissiles", fileName = "FireMissiles")]
public class FireMissilesNode : LeafNodeData
{
    [Tooltip("The AI will not fire missiles beyond this range.")]
    [SerializeField] protected float maxRange = 50;
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        //Debug.Log("A");
        var launchers = ctx.GetData<List<MissileLauncher>>(Blackboard.Keys.Launchers);
        //Debug.Log(ctx.GetData<List<MissileLauncher>>(Blackboard.Keys.Launchers) == null);
        if (launchers.IsEmpty()) return NodeState.FAILURE;
        //Debug.Log("B");
        var target = ctx.GetData<Unit>(Blackboard.Keys.Target);
        if (target == null) return NodeState.FAILURE;
        if (!target.gameObject.activeSelf)
        {
            ctx.SetData<Unit>(Blackboard.Keys.Target, null);
            return NodeState.FAILURE;
        }
        if (Vector3.Distance(ctx.Position, target.Position) > maxRange || !ctx.Unit.InOptimalFiringRange(target)) return NodeState.FAILURE;
        //Debug.Log("C");
        Debug.Log($"{ctx.Transform} shooting missiles!");
        foreach (var l in launchers)
        {
            l.Shoot(target);
        }

        return NodeState.RUNNING;
    };
}