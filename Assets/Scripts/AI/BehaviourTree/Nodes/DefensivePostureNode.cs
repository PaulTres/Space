using HybridBT2;
using System;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/DefensivePosture", fileName = "DefensivePosture")]
public class DefensivePostureNode : LeafNodeData
{
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        throw new NotImplementedException();
        foreach (var turret in ctx.GetData<List<Turret>>(Blackboard.Keys.AllTurrets))
        {
            //compute the value of each turret and decide if it should fire or not
        }
        return NodeState.RUNNING;
    };

    protected float TurretScore(Turret turret, Blackboard blackboard)
    {
        throw new NotImplementedException();
    }
}
