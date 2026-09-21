using UnityEditor;
using UnityEngine;
using Weapons;

[CustomEditor(typeof(Turret))]
public class TurretEditor : Editor
{
    private void OnSceneGUI()
    {
        Turret turret = (Turret)target;
        if (turret.Parameters == null) return;
        if (turret.Parameters.MaxAngleDifference >= 360) return;
        Handles.color = Color.softGreen;
        var maxRotation = turret.MaxRot;
        var minRotation = turret.MinRot;
        var maxRotPos = Mathf.Sin(maxRotation * Mathf.Deg2Rad) * turret.RotationPivot.right + Mathf.Cos(maxRotation * Mathf.Deg2Rad) * turret.RotationPivot.forward;
        var minRotPos = Mathf.Sin(minRotation * Mathf.Deg2Rad) * turret.RotationPivot.right + Mathf.Cos(minRotation * Mathf.Deg2Rad) * turret.RotationPivot.forward;
        Handles.DrawLine(turret.RotationPivot.position + maxRotPos, turret.RotationPivot.position);
        Handles.DrawLine(turret.RotationPivot.position, turret.RotationPivot.position + minRotPos);
        Handles.DrawWireArc(turret.RotationPivot.position, turret.RotationPivot.up, minRotPos, Mathf.Abs(maxRotation) + Mathf.Abs(minRotation), 1);
        Handles.color = Color.softRed;
        var maxElevation = turret.MaxElevation;
        var minElevation = turret.MinElevation;
        var maxElevationPos = Mathf.Sin(maxElevation * Mathf.Deg2Rad) * turret.ElevationPivot.up + Mathf.Cos(maxElevation * Mathf.Deg2Rad) * turret.ElevationPivot.forward;
        var minElevationPos = Mathf.Sin(minElevation * Mathf.Deg2Rad) * turret.ElevationPivot.up + Mathf.Cos(minElevation * Mathf.Deg2Rad) * turret.ElevationPivot.forward;
        Handles.DrawLine(turret.ElevationPivot.position + maxElevationPos, turret.ElevationPivot.position);
        Handles.DrawLine(turret.ElevationPivot.position, turret.ElevationPivot.position + minElevationPos);
        Handles.DrawWireArc(turret.ElevationPivot.position, turret.ElevationPivot.right, maxElevationPos, Mathf.Abs(maxElevation) + Mathf.Abs(minElevation), 1);
    }
}
