using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugService", menuName = "HybridBT2/Services/DebugService")]
public class DebugService : ServiceData
{
    public override Action<Node, Blackboard> Action => (n, ctx) =>
    {
        Debug.Log($"DebugService: Node={n.Name}, {n.State}");
    };
}
