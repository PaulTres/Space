using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/Flee", fileName = "Flee")]
public class FleeNode : LeafNodeData
{
    [SerializeField] protected float fleeDistance = 1000;

    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        ctx.Navigation.RotationTarget = null;
        ctx.SetData(Blackboard.Keys.Fleeing, true);
    };

    protected override Action<Node, Blackboard> onExit => (n, ctx) =>
    {
        ctx.SetData(Blackboard.Keys.Fleeing, false);
    };

    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        //no need to normalize here, we just need the direction
        //this could probably be optimized to only happen once every few ticks
        ctx.Navigation.SetDestination(ctx.Navigation.Position + (ctx.Navigation.Position -
            target.Position) * fleeDistance);
        return NodeState.RUNNING;
    };
}
