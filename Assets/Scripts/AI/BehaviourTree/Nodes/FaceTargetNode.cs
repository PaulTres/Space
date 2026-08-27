using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/FaceTargetNode", fileName = "FaceTargetNode")]
public class FaceTargetNode : LeafNodeData
{
    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        ctx.Navigation.RotationTarget = new FaceTargetRotation(Blackboard.Keys.Target);
    };

    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        return NodeState.RUNNING;
    };
}
