using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/EscortNode", fileName = "EscortNode")]
public class EscortNode: LeafNodeData
{
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var ships = UnitManager.GetShips(ctx.Navigation.Team);
        foreach (var ship in ships)
        {
            if (!ship.Active()|| ship == ctx.Navigation||ship.IsDying||!ship.NeedsEscort) continue;
            ctx.Navigation.SetDestination(ship.Position);
            return NodeState.RUNNING;
        }
        return NodeState.FAILURE;
    };
}
