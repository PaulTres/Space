using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/EvadeThreat", fileName = "EvadeThreat")]
public class EvadeThreatNode : LeafNodeData
{
    [SerializeField] protected float directionMagnitude = 10;
    [SerializeField] protected float minDotProduct = -.9f;
    [SerializeField] protected bool resetRotationTarget = true;

    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        if (resetRotationTarget) ctx.Navigation.RotationTarget = null;
    };

    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        if (ctx.Threats.IsEmpty()) return NodeState.FAILURE;
        Vector3 direction = Vector3.zero;
        foreach (var t in ctx.Threats)
        {
            var dodgeDir = t.DodgeDirection(ctx.Navigation);
            if (Vector3.Dot(dodgeDir, direction.normalized) >= minDotProduct)
            {
                direction += dodgeDir;
            }
            else direction = Vector3.Cross(direction, dodgeDir);
        }
        ctx.Navigation.SetDestination(ctx.Position + direction * directionMagnitude);
        return NodeState.RUNNING;
    };
}
