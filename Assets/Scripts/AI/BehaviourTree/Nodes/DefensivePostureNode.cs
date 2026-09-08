using HybridBT2;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/DefensivePosture", fileName = "DefensivePosture")]
public class DefensivePostureNode : LeafNodeData
{
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        if (!target.Active()) return NodeState.FAILURE;
        foreach (var turret in ctx.GetData<List<Turret>>(Blackboard.Keys.AllTurrets))
        {
            var speedScore = Mathf.Max(turret.BulletSpeed / target.MaxVelocityMagnitude, 1);
            //calculate the damage vs the signature of the weapon; also account for point defense turrets
            var dps = turret.DPS;
        }
        return NodeState.RUNNING;
    };
}
