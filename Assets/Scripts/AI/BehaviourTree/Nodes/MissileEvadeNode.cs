using HybridBT2;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/MissileEvade", fileName = "Evade")]
public class MissileEvadeNode : LeafNodeData
{
    [SerializeField] protected float directionMagnitude = 10;
    [SerializeField] protected bool resetRotationTarget = true;

    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        if (resetRotationTarget) ctx.Navigation.RotationTarget = null;
    };

    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var pdTurrets = ctx.GetData<IEnumerable<Turret>>(Blackboard.Keys.PDTurrets);
        if (pdTurrets == null) return NodeState.FAILURE;
        foreach (var turret in pdTurrets)
        {
            if (turret.Target == null) continue;
            var m = turret.Target as Missile;
            if (m != null && m.gameObject.activeSelf)
            {
                var normal = m.Transform.position - ctx.Position;
                var direction = Vector3.ProjectOnPlane(normal, ctx.Navigation.Forward).normalized;
                if (direction.magnitude == 0) direction = normal.Perpendicular();
                ctx.Navigation.SetDestination(ctx.Position + direction * directionMagnitude);
                return NodeState.RUNNING;
            }
        }
        return NodeState.FAILURE;
    };
}
