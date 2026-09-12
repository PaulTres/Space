using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/Dummy", fileName = "Dummy")]
public class DummyNode : LeafNodeData 
{
    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) => NodeState.RUNNING;

}
