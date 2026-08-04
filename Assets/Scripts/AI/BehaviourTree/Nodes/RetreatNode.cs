using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/Retreat", fileName = "Retreat")]
public class RetreatNode : LeafNodeData
{
    [SerializeField] protected bool maintainFacing;
    [SerializeField] protected float retreatDistance = 1000;
    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        if (!maintainFacing) ctx.Navigation.RotationTarget = null;
    };

    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        //Debug.Log("Retreating");
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        //no need to normalize here, we just need the direction
        //this could probably be optimized to only happen once every few ticks
        ctx.Navigation.SetDestination(ctx.Navigation.Position + (ctx.Navigation.Position -
            target.Position) * retreatDistance);
        return NodeState.RUNNING;
    };
}
