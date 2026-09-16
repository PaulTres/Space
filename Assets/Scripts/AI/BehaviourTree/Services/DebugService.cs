using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugService", menuName = "HybridBT2/Services/DebugService")]
public class DebugService : ServiceData
{
    public override Action<Node, Blackboard> Action => (n, ctx) =>
    {
        if (n.Parent == null) Debug.Log($"DebugService: Node={n.Name}, {n.State}");
        else Debug.Log($"DebugService: Node={n.Name}, {n.State} with parent {n.Parent.Name}");
    };
}
