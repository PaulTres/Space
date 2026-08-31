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
        var maxRotation = turret.Parameters.MaxRot;
        var minRotation = turret.Parameters.MinRot;
        var maxRotPos = Mathf.Sin(maxRotation * Mathf.Deg2Rad) * turret.Transform.right + Mathf.Cos(maxRotation * Mathf.Deg2Rad) * turret.transform.forward;
        var minRotPos = Mathf.Sin(minRotation * Mathf.Deg2Rad) * turret.Transform.right + Mathf.Cos(minRotation * Mathf.Deg2Rad) * turret.transform.forward;
        Handles.DrawLine(turret.Transform.position + maxRotPos, turret.Transform.position);
        Handles.DrawLine(turret.Transform.position, turret.Transform.position + minRotPos);
        Handles.DrawWireArc(turret.Transform.position, turret.Transform.up, minRotPos, Mathf.Abs(maxRotation) + Mathf.Abs(minRotation), 1);
        Handles.color = Color.softRed;
        var maxElevation = turret.Parameters.MaxElevation;
        var minElevation = turret.Parameters.MinElevation;
        var maxElevationPos = Mathf.Sin(maxElevation * Mathf.Deg2Rad) * turret.Transform.up + Mathf.Cos(maxElevation * Mathf.Deg2Rad) * turret.transform.forward;
        var minElevationPos = Mathf.Sin(minElevation * Mathf.Deg2Rad) * turret.Transform.up + Mathf.Cos(minElevation * Mathf.Deg2Rad) * turret.transform.forward;
        Handles.DrawLine(turret.Transform.position + maxElevationPos, turret.Transform.position);
        Handles.DrawLine(turret.Transform.position, turret.Transform.position + minElevationPos);
        Handles.DrawWireArc(turret.Transform.position, turret.Transform.right, maxElevationPos, Mathf.Abs(maxElevation) + Mathf.Abs(minElevation), 1);
    }
}
