using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/EscortNode", fileName = "EscortNode")]
public class EscortNode : LeafNodeData
{
    [SerializeField] protected float escortDistance = 20;
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var candidate = ctx.GetData<Unit>(Blackboard.Keys.EscortCandidate);
        if (!candidate.Active()) return NodeState.FAILURE;
        ctx.Navigation.SetDestination((ctx.Position - candidate.Position).normalized * escortDistance);
        return NodeState.RUNNING;
    };
}
