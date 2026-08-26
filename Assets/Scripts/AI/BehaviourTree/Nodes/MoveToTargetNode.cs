using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/MoveToTarget", fileName = "MoveToTarget")]
public class MoveToTargetNode : LeafNodeData
{
    public Blackboard.Keys TargetKey;
    public float StopDistance = 0.5f, TargetErrorThreshold = .5f;
    public bool ReturnSuccessOnReach = false;
    protected void OnValidate()
    {
        if (StopDistance < TargetErrorThreshold)
        {
            Debug.LogWarning($"Stop distance is greater than target error threshold on {Name}.");
        }
    }
    protected enum Keys
    {
        OldStopDistance,
        PrevTargetPos
    }
    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        var tr = ctx.GetData<Transform>(TargetKey);
        if (tr == null)
        {
            Debug.LogError("Cannot pathfind to null target!");
            return;
        }
        ctx.Navigation.RotationTarget = null;
        ((LeafNode)n).SetData((int)Keys.OldStopDistance, ctx.Navigation.StopDistance);
        ((LeafNode)n).SetData((int)Keys.PrevTargetPos, tr.position);
        ctx.Navigation.StopDistance = StopDistance;
        ctx.Navigation.SetDestination(tr.position);
    };
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var tr = ctx.GetData<Transform>(TargetKey);
        if (tr == null)
        {
            Debug.LogError("Cannot pathfind to null target!");
            return NodeState.FAILURE;
        }

        var prevPos = n.GetData<Vector3>((int)Keys.PrevTargetPos);

        if (Vector3.Distance(prevPos, tr.position) > TargetErrorThreshold)
        {
            n.SetData((int)Keys.PrevTargetPos, tr.position);
            ctx.Navigation.SetDestination(tr.position);
            return NodeState.RUNNING;
        }

        if (ReturnSuccessOnReach && Vector3.Distance(ctx.Position, tr.position) <
            ctx.Navigation.StopDistance)
        {
            return NodeState.SUCCESS;
        }

        return NodeState.RUNNING;
    };
    protected override Action<Node, Blackboard> onExit => (n, ctx) =>
    {
        ctx.Navigation.SetDestination(null);
        ctx.Navigation.StopDistance = ((LeafNode)n).GetData<float>((int)Keys.OldStopDistance);
    };
}
