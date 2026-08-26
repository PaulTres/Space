using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/EscortNode", fileName = "EscortNode")]
public class EscortNode : LeafNodeData
{
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var candidate = ctx.GetData<Unit>(Blackboard.Keys.EscortCandidate);
        if (candidate == null) return NodeState.FAILURE;
        ctx.Navigation.SetDestination(candidate.Position);
        return NodeState.RUNNING;
    };
}
