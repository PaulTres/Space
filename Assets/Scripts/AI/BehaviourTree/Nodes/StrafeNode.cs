using HybridBT2;
using System;
using UnityEngine;
using Utilities;
[CreateAssetMenu(menuName = "HybridBT2/Leaves/Strafe", fileName = "Strafe")]
public class StrafeNode : LeafNodeData
{
    public enum TargetFacingMode
    {
        Broadside,
        Forward,
        Vertical
    }

    public TargetFacingMode TargetFacing;
    public int TicksBeforeRepath = 10;

    protected enum InternalKeys
    {
        CurrentTicks,
        PrevDirection
    }

    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        ((LeafNode)n).SetData((int)InternalKeys.CurrentTicks, 2 * TicksBeforeRepath);
        switch (TargetFacing)
        {
            case TargetFacingMode.Broadside:
                ctx.Navigation.RotationTarget = new BroadsideFacingRotation(Blackboard.Keys.Target);
                break;
            case TargetFacingMode.Forward:
                ctx.Navigation.RotationTarget = new FaceTargetRotation(Blackboard.Keys.Target);
                break;
            case TargetFacingMode.Vertical:
                ctx.Navigation.RotationTarget = new VerticalFacingRotation(Blackboard.Keys.Target);
                break;
        }
    };

    protected override Action<Node, Blackboard> onExit => (n, ctx) =>
    {
        ctx.Navigation.RotationTarget = null;
    };

    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var target = ctx.GetData<Unit>(Blackboard.Keys.Target);
        if (!target.Active()) return NodeState.FAILURE;
        //Debug.Log("Strafing");

        var currentTicks = n.GetData<int>((int)InternalKeys.CurrentTicks);
        if (currentTicks >= TicksBeforeRepath)
        {
            n.SetData((int)InternalKeys.CurrentTicks, 0);
            switch (TargetFacing)
            {
                case TargetFacingMode.Forward:
                    ctx.Navigation.SetDestination(target.Transform.position +
                        target.Transform.right * 1000);
                    break;
                default:
                    ctx.Navigation.SetDestination(ctx.Navigation.Transform.position +
                        ctx.Navigation.Transform.forward * 1000);
                    break;
            }
        }
        else
        {
            n.SetData((int)InternalKeys.CurrentTicks, currentTicks + 1);
        }

        return NodeState.RUNNING;
    };
}
