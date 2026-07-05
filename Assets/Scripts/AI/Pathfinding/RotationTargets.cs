using HybridBT2;
using UnityEngine;
using Utilities;

public abstract class RotationTarget
{
    public abstract Quaternion GetTargetRotation(Transform unit);
}

public class FaceTargetRotation : RotationTarget
{
    protected Blackboard.Keys TargetKey;

    public FaceTargetRotation(Blackboard.Keys targetKey)
    {
        TargetKey = targetKey;
    }

    public override Quaternion GetTargetRotation(Transform unit)
    {
        var tr = unit.GetBlackboard().GetData<IObject>(TargetKey);
        if (tr == null) return unit.rotation;

        var direction = (tr.Position - unit.position).normalized;
        return Quaternion.LookRotation(direction, direction + unit.up);
    }
}

public class VerticalFacingRotation : RotationTarget
{
    protected Blackboard.Keys TargetKey;
    public VerticalFacingRotation(Blackboard.Keys targetKey)
    {
        TargetKey = targetKey;
    }
    public override Quaternion GetTargetRotation(Transform unit)
    {
        var tr = unit.GetBlackboard().GetData<IObject>(TargetKey);
        if (tr == null) return unit.rotation;
        throw new System.NotImplementedException();
    }
}

public class BroadsideFacingRotation : RotationTarget
{
    protected Blackboard.Keys TargetKey;
    public BroadsideFacingRotation(Blackboard.Keys targetKey)
    {
        TargetKey = targetKey;
    }
    public override Quaternion GetTargetRotation(Transform unit)
    {
        var tr = unit.GetBlackboard().GetData<IObject>(TargetKey);
        if (tr == null) return unit.rotation;
        var dir = tr.Position - unit.position;
        var upDir = dir.Perpendicular();
        var forward = Vector3.Cross(upDir, dir).normalized;
        return Quaternion.LookRotation(forward, upDir);
    }
}