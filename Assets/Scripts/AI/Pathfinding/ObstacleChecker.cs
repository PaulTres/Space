using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Obstacle Checker", fileName = "ObstacleChecker")]
public class ObstacleChecker : ScriptableObject
{
    public bool Debug = true;
    public LayerMask collisionMask = 1 << 0 | 1 << 11 | 1 << 12;
    public float collisionCheckRadius = 1f, collisionCheckDistance = 1f;
    public int[] offsetPointsZ = new int[3] { 1, 0, -1 },
        offsetPointsX = new int[3] { 0, 1, -1 },
        offsetPointsY = new int[3] { 0, 1, -1 };

    /// <summary>
    /// Returns true if an obstacle has been encountered.
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public bool CheckForward(Navigation origin, Vector3 offset, Collider[] buffer)
    {
        int count = Physics.OverlapCapsuleNonAlloc(origin.Position + offset,
            origin.Position + offset * (collisionCheckDistance +
            origin.LinearVelocity.magnitude), collisionCheckRadius,
            buffer, collisionMask);
        for (int i = 0; i < count; i++)
        {
            if (buffer[i].transform.root.GetInstanceID() == origin.Transform.root.GetInstanceID()) continue;
            return true;
        }
        return false;
    }

    public Vector3 RunCheck(Navigation navigation, Collider[] buffer)
    {
        var origin = navigation.NavPivot;
        for (int i = 0; i < offsetPointsZ.Length; i++)
        {
            for (int j = 0; j < offsetPointsX.Length; j++)
            {
                for (int k = 0; k < offsetPointsY.Length; k++)
                {
                    int z = offsetPointsZ[i], x = offsetPointsX[j], y = offsetPointsY[k];
                    if ((z == 0 || z == 1) && x == 0 && y == 0) continue;

                    var offset = (z * origin.forward + x * origin.right + y * origin.up).normalized;
                    var point = origin.position + collisionCheckDistance * offset;

                    var n = Physics.OverlapSphereNonAlloc(point, collisionCheckRadius,
                        buffer, collisionMask);
                    bool foreignObstacle = false;
                    for (int p = 0; p < n && !foreignObstacle; p++)
                    {
                        if (buffer[p].transform.root.GetInstanceID() != origin.root.GetInstanceID())
                        {
                            foreignObstacle = true;
                        }
                    }

                    if (!foreignObstacle) return offset;
                }
            }
        }
        return Vector3.zero;
    }

    public void Draw(Transform origin, Collider[] buffer)
    {
        if (!Debug) return;
        for (int i = 0; i < offsetPointsZ.Length; i++)
        {
            for (int j = 0; j < offsetPointsX.Length; j++)
            {
                for (int k = 0; k < offsetPointsY.Length; k++)
                {
                    int z = offsetPointsZ[i], x = offsetPointsX[j], y = offsetPointsY[k];
                    if (z == 0 && x == 0 && y == 0) continue;

                    var offset = (z * origin.forward + x * origin.right + y * origin.up).normalized;
                    var point = origin.position + collisionCheckDistance * offset;

                    var n = Physics.OverlapSphereNonAlloc(point, collisionCheckRadius,
                        buffer, collisionMask);
                    bool foreignObstacle = false;
                    for (int p = 0; p < n && !foreignObstacle; p++)
                    {
                        if (buffer[p].transform.root.GetInstanceID() != origin.root.GetInstanceID())
                        {
                            foreignObstacle = true;
                            UnityEngine.Debug.Log(buffer[p].transform.root);
                        }
                    }

                    Gizmos.color = foreignObstacle ? Color.red : Color.white;

                    Gizmos.DrawLine(origin.position, point);
                    Gizmos.DrawWireSphere(point, collisionCheckRadius);
                }
            }
        }
    }
}
