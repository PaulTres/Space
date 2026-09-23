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
    public int TicksBeforeRepath = 10;
    [SerializeField] protected float strafeMagnitude = 10;
    public bool UseOptimalFiringRange = true;
    [SerializeField] protected float distanceBuffer = 0;
    [SerializeField]
    protected StrafeDirection[] strafeDirections = new StrafeDirection[3]
    {
        StrafeDirection.Forward,StrafeDirection.Horizontal,StrafeDirection.Vertical
    };
    /// <summary>
    /// The strafe distance will never exceed this value. Used to prevent ships from strafing outside their weapon range.
    /// </summary>
    [SerializeField] protected float maxDistance = 80;

    protected enum InternalKeys
    {
        CurrentTicks,
        PrevDirection
    }

    protected Vector3 GetStrafeDirection(Transform transform, StrafeDirection strafeDirection)
    {
        return strafeDirection switch
        {
            StrafeDirection.Horizontal => transform.right,
            StrafeDirection.Vertical => transform.up,
            StrafeDirection.Forward => transform.forward,
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
                strafeDirection = Vector3.ProjectOnPlane(GetStrafeDirection(ctx.Navigation.Transform, strafeDirections[i]), normal);
                if (strafeDirection.magnitude > 0.3f) break;
            }
            //Debug.Log("Strafing");
            var destination = ctx.Position + strafeDirection * strafeMagnitude;
            //Debug.DrawRay(ctx.Position, strafeDirection * strafeMagnitude, Color.yellow, 2);
            var targetToDestination = (destination - target.Position).normalized;
            var dist = Mathf.Min(UseOptimalFiringRange ? target.CurrentSignature +
                ctx.Navigation.ScanRange + distanceBuffer : distanceBuffer, maxDistance);
            destination = target.Position + targetToDestination * dist;
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
