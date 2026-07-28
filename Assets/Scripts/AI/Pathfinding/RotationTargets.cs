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
        var dir = tr.Position - unit.position;
        var forward = dir.Perpendicular();
        return Quaternion.LookRotation(forward, dir.normalized);
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
        if (tr == null) return Quaternion.LookRotation(unit.forward, unit.up);
        var dir = tr.Position - unit.position;
        var forward = Vector3.ProjectOnPlane(unit.forward, dir.normalized).normalized;
        if (forward.magnitude == 0) forward = unit.right;
        var upDir = Vector3.Cross(dir.normalized, forward);
        return Quaternion.LookRotation(forward, upDir);
    }
}