using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/Intercept", fileName = "Intercept")]
public class InterceptNode : LeafNodeData
{
    public float InterceptError = 1;
    public bool UseOptimalFiringRange = true;
    public float distanceBuffer = 0;
    protected enum InternalKeys
    {
        TargetPrevPos
    }

    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        ctx.Navigation.RotationTarget = new FaceTargetRotation(Blackboard.Keys.Target);
        var target = ctx.GetData<Unit>(Blackboard.Keys.Target);
        if (target.Active())
        {
            ((LeafNode)n).SetData((int)InternalKeys.TargetPrevPos, target.Position);
            var dir = ctx.Position - target.Position;
            var dist = UseOptimalFiringRange ? target.CurrentSignature +
                ctx.Navigation.ScanRange + distanceBuffer : distanceBuffer;
            ctx.Navigation.SetDestination(target.Position + dir.normalized * dist);
        }
    };
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        if (!target.Active()) return NodeState.FAILURE;

        var targetPos = target.Position;
        if ((targetPos - n.GetData<Vector3>((int)InternalKeys.TargetPrevPos)).sqrMagnitude > InterceptError * InterceptError)
        {
            n.SetData((int)InternalKeys.TargetPrevPos, targetPos);
            var dir = ctx.Position - target.Position;
            var dist = UseOptimalFiringRange ? target.CurrentSignature +
                ctx.Navigation.ScanRange + distanceBuffer : distanceBuffer;
            ctx.Navigation.SetDestination(target.Position + dir.normalized * dist);
        }
        return NodeState.RUNNING;
    };
}
