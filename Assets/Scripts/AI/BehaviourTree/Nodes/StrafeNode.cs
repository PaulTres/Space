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
    [SerializeField] protected float strafeMagnitude = 100;
    [SerializeField] protected float strafeDistance = 50;

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
            var normal = target.Position - ctx.Position;
            var destination = Vector3.zero;
            switch (TargetFacing)
            {
                case TargetFacingMode.Forward:
                    {
                        destination = Vector3.ProjectOnPlane(ctx.Navigation.Right, normal);
                        if (destination.magnitude == 0) destination = ctx.Navigation.Up;
                        break;
                    }
                default:
                    {
                        destination = Vector3.ProjectOnPlane(ctx.Navigation.Forward, normal);
                        if (destination.magnitude == 0) destination = ctx.Navigation.Right;
                        break;
                    }
            }
            destination = ctx.Position + destination.normalized * strafeMagnitude;
            var dirToTarget = destination - target.Position;
            ctx.Navigation.SetDestination(dirToTarget.normalized * strafeDistance);
        }
        else
        {
            n.SetData((int)InternalKeys.CurrentTicks, currentTicks + 1);
        }
        return NodeState.RUNNING;
    };
}
