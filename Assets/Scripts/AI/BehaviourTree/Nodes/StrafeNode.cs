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
    [SerializeField] protected float strafeMagnitude = 50;
    public bool UseOptimalFiringRange = true;
    [SerializeField] protected float distanceBuffer = 0;

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
            var strafeDirection = Vector3.zero;
            switch (TargetFacing)
            {
                case TargetFacingMode.Forward:
                    {
                        strafeDirection = Vector3.ProjectOnPlane(ctx.Navigation.Right, normal);
                        if (strafeDirection.magnitude < 0.1f) strafeDirection = ctx.Navigation.Up;
                        Debug.DrawRay(ctx.Position, strafeDirection, Color.white, 2);
                        break;
                    }
                default:
                    {
                        strafeDirection = Vector3.ProjectOnPlane(ctx.Navigation.Forward, normal);
                        if (strafeDirection.magnitude < .1f) strafeDirection = ctx.Navigation.Right;
                        break;
                    }
            }
            //Debug.Log("Strafing");
            var destination = ctx.Position + strafeDirection * strafeMagnitude;
            //Debug.DrawRay(ctx.Position, strafeDirection * strafeMagnitude, Color.yellow, 2);
            var targetToDestination = (destination - target.Position).normalized;
            var dist = UseOptimalFiringRange ? target.CurrentSignature +
                ctx.Navigation.ScanRange + distanceBuffer : distanceBuffer;
            destination = target.Position + targetToDestination * dist;
            //Debug.DrawLine(ctx.Position, destination, Color.blue);
            ctx.Navigation.SetDestination(destination);
        }
        else
        {
            n.SetData((int)InternalKeys.CurrentTicks, currentTicks + 1);
        }
        return NodeState.RUNNING;
    };
}
