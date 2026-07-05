using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/Intercept", fileName = "Intercept")]
public class InterceptNode : LeafNodeData
{
    public float InterceptError = 1;
    protected enum InternalKeys
    {
        TargetPrevPos
    }
    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        ctx.Navigation.RotationTarget = new FaceTargetRotation(Blackboard.Keys.Target);
        var target = ctx.GetData<IObject>(Blackboard.Keys.Target);
        if (target != null && target.Active)
        {
            ((LeafNode)n).SetData((int)InternalKeys.TargetPrevPos, target.Position);
            ctx.Navigation.SetDestination(target.Position);
        }
    };
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var target = ctx.GetData<IObject>(Blackboard.Keys.Target);
        if (target == null) return NodeState.FAILURE;
        if (!target.Transform.gameObject.activeSelf)
        {
            ctx.SetData<IObject>(Blackboard.Keys.Target, null);
            return NodeState.FAILURE;
        }

        var targetPos = target.Transform.position;
        if ((targetPos - n.GetData<Vector3>((int)InternalKeys.TargetPrevPos)).sqrMagnitude > InterceptError * InterceptError)
        {
            n.SetData((int)InternalKeys.TargetPrevPos, targetPos);
            ctx.Navigation.SetDestination(targetPos);
        }
        return NodeState.RUNNING;
    };
}
