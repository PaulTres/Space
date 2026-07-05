using HybridBT2;
using System;
using UnityEngine;
using Utilities;
/// <summary>
/// This node handles basic strafing. It does NOT handle ship facing.
/// </summary>
[CreateAssetMenu(menuName = "HybridBT2/Leaves/BasicStrafe", fileName = "BasicStrafe")]
public class BasicStrafeNode : LeafNodeData
{
    protected readonly struct Triangle
    {
        public readonly Vector3 PrevPoint1, PrevPoint2;
        public Triangle(Vector3 prevPoint1, Vector3 prevPoint2)
        {
            PrevPoint1 = prevPoint1;
            PrevPoint2 = prevPoint2;
        }
    }
    protected enum Keys
    {
        PrevTargetPos,
        PrevPoint,
        OldStopDistance
    }
    [Tooltip("Object to circle around. Must be a Unit.")]
    public Blackboard.Keys TargetKey;
    public float StopDistance = 0.5f, Radius = 10, TargetErrorThreshold = 1;
    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        ctx.Navigation.SetDestination(null);
        ((LeafNode)n).SetData((int)Keys.OldStopDistance, ctx.Navigation.StopDistance);
        ctx.Navigation.StopDistance = StopDistance;
        ((LeafNode)n).SetData<Vector3?>((int)Keys.PrevPoint, null);
    };
    protected void Repath(LeafNode n, Blackboard ctx)
    {
        var tr = ctx.GetData<Unit>(TargetKey);
        n.SetData((int)Keys.PrevTargetPos, tr.Position);
        var t = n.GetData<Triangle?>((int)Keys.PrevPoint);
        if (t == null)
        {
            var p1 = (ctx.Transform.position - tr.Position).normalized * Radius;
            var p2 = p1.Perpendicular() * Radius;
            var p3 = ((p1 + p2) / 2).normalized * Radius;
            ctx.Navigation.SetDestination(tr.Position + p3);

            n.SetData<Triangle>((int)Keys.PrevPoint, new(p1, p3));
        }
        else
        {
            //Debug.DrawLine(t.Value.PrevPoint1 + tr.position, t.Value.PrevPoint2 + tr.position, Color.yellow, 10);

            var v = Vector3.Project(t.Value.PrevPoint1 - t.Value.PrevPoint2, -t.Value.PrevPoint2);

            var point = v + t.Value.PrevPoint2;

            var p = point + point - t.Value.PrevPoint1;

            //Debug.DrawLine(t.Value.PrevPoint2 + tr.position, tr.position + p, Color.green, 10);


            ctx.Navigation.SetDestination(tr.Position + p);

            n.SetData<Triangle>((int)Keys.PrevPoint, new(t.Value.PrevPoint2, p));
        }
    }
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var tr = ctx.GetData<Unit>(TargetKey);
        if (tr == null) return NodeState.FAILURE;
        var prevPos = n.GetData<Vector3?>((int)Keys.PrevTargetPos);
        if (prevPos == null || !ctx.Navigation.HasDestination ||
            Vector3.Distance(prevPos.Value, tr.Position) > TargetErrorThreshold)
        {
            //find a new point around the target to move to
            Repath(n, ctx);
        }
        return NodeState.RUNNING;
    };
    protected override Action<Node, Blackboard> onExit => (n, ctx) =>
    {
        ctx.Navigation.SetDestination(null);
        ctx.Navigation.StopDistance = ((LeafNode)n).GetData<float>((int)Keys.OldStopDistance);
    };
}
