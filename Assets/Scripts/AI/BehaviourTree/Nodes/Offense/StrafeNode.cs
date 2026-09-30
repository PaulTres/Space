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

    public enum StrafeDirection
    {
        Vertical,
        Horizontal,
        Forward
    }

    public TargetFacingMode TargetFacing;
    public int TicksBeforeRepath = 2;
    [SerializeField] protected float strafeMagnitude = 50;
    public bool UseOptimalFiringRange = true;
    [SerializeField] protected float distanceBuffer = 0;
    [SerializeField]
    protected StrafeDirection[] strafeDirections = new StrafeDirection[3]
    {
        StrafeDirection.Forward,StrafeDirection.Horizontal,StrafeDirection.Vertical
    };
    [Tooltip("The strafe distance will never exceed this value. Used to prevent ships from strafing outside their weapon range.")]
    [SerializeField] protected float maxDistance = 80;
    [Tooltip("The distance between the initial destination and intended destination will never exceed this value.")]
    [SerializeField] protected float maxDistanceDelta = 10;
    [SerializeField] protected float attractionTowardsCenter = 10;

    protected enum InternalKeys
    {
        CurrentTicks,
        PrevDirection
    }

    protected Vector3 GetStrafeDirection(Ship ship, StrafeDirection strafeDirection)
    {
        return strafeDirection switch
        {
            StrafeDirection.Horizontal => ship.Right,
            StrafeDirection.Vertical => ship.Up,
            StrafeDirection.Forward => ship.Forward,
            _ => throw new Exception($"Invalid strafe direction: {strafeDirection}")
        };
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
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        if (!target.Active()) return NodeState.FAILURE;
        //if (Name.Contains("Medium")) Debug.Log("Strafing " + target);

        var currentTicks = n.GetData<int>((int)InternalKeys.CurrentTicks);
        if (currentTicks >= TicksBeforeRepath)
        {
            n.SetData((int)InternalKeys.CurrentTicks, 0);
            var normal = target.Position - ctx.Position;
            var strafeDirection = Vector3.zero;

            for (int i = 0; i < strafeDirections.Length; i++)
            {
                strafeDirection = Vector3.ProjectOnPlane(GetStrafeDirection(ctx.Navigation, strafeDirections[i]), normal);
                if (strafeDirection.magnitude > 0.3f) break;
            }
            //Debug.Log("Strafing");
            var destination = ctx.Position + strafeDirection * strafeMagnitude;
            //Debug.DrawRay(ctx.Position, strafeDirection * strafeMagnitude, Color.yellow, 2);
            var targetToDestination = destination - target.Position;
            var dist = Mathf.Min(UseOptimalFiringRange ? target.CurrentSignature +
                ctx.Navigation.ScanRange + distanceBuffer : distanceBuffer, maxDistance);
            destination = target.Position + targetToDestination.normalized * Mathf.Min(dist, targetToDestination.magnitude + maxDistanceDelta);
            destination -= destination.normalized * attractionTowardsCenter;
            //Debug.DrawLine(ctx.Position, destination, Color.blue);
            ctx.Navigation.SetDestination(destination);
            //Debug.Log(Vector3.Distance(destination, target.Position) + "-:-" + dist);
        }
        else
        {
            n.SetData((int)InternalKeys.CurrentTicks, currentTicks + 1);
        }
        return NodeState.RUNNING;
    };
}
