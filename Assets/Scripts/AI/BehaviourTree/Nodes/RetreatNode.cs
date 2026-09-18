using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/Retreat", fileName = "Retreat")]
public class RetreatNode : LeafNodeData
{
    [SerializeField] protected bool maintainFacing = true;
    [SerializeField] protected float randomRadius = 1;

    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        if (!maintainFacing)
        {
            ctx.Navigation.RotationTarget = null;
            //Debug.Log("A");
        }
        ((LeafNode)n).SetData(0, UnityEngine.Random.insideUnitSphere * randomRadius);
    };


    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        //Debug.Log("Retreating");
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        if (!target.InOptimalFiringRange(ctx.Navigation))
        {
            //Debug.Log($"Retreat successfull!{Vector3.Distance(target.Position, ctx.Position)}-:-{target.ScanRange + ctx.Navigation.CurrentSignature}");
            return NodeState.SUCCESS;
        }
        //no need to normalize here, we just need the direction
        //this could probably be optimized to only happen once every few ticks
        ctx.Navigation.SetDestination(ctx.Navigation.Position + (ctx.Navigation.Position -
            target.Position) * 10 + n.GetData<Vector3>(0));
        return NodeState.RUNNING;
    };
}
