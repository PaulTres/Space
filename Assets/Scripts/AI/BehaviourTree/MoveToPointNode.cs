using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/MoveToPoint", fileName = "MoveToPoint")]
public class MoveToPointNode : LeafNodeData
{
    public Blackboard.Keys TargetPositionKey = Blackboard.Keys.TargetPosition;

    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        var targetPos = ctx.GetData<Vector3>(TargetPositionKey);
        ctx.Navigation.SetDestination(targetPos);
    };

    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        if (ctx.Navigation.Destination == null) return NodeState.SUCCESS;
        return NodeState.RUNNING;
    };
}
