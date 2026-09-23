using HybridBT2;
using UnityEngine;
using Utilities;

public class StationaryObject : Unit
{
    /// <summary>
    /// The unit this is currently targeting.
    /// </summary>
    public override ImprovedMonoBehaviour Target
    {
        get
        {
            var blackboard = Transform.GetBlackboard();
            if (blackboard == null) return null;
            return blackboard.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        }
    }
}
