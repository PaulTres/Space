using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Obstacle Checker", fileName = "ObstacleChecker")]
public class ObstacleChecker : ScriptableObject
{
    public bool Debug = false;
    public LayerMask collisionMask = 1 << 0 | 1 << 6 | 1 << 8;
    public float collisionCheckRadius = 1f, collisionCheckDistance = 1f;
    readonly Collider[] hitColliders = new Collider[10];
    public int[] offsetPointsZ = new int[3] { 1, 0, -1 },
        offsetPointsX = new int[3] { 0, 1, -1 },
        offsetPointsY = new int[3] { 0, 1, -1 };
    public bool CheckForward(Transform origin, Vector3 offset)
    {
        return Physics.CheckSphere(origin.position + offset * collisionCheckDistance, collisionCheckRadius, collisionMask);
    }
    public Vector3 RunCheck(Transform origin)
    {
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
                        hitColliders, collisionMask);
                    bool foreignObstacle = false;
                    for (int p = 0; p < n && !foreignObstacle; p++)
                    {
                        if (hitColliders[p].transform.root.GetInstanceID() != origin.root.GetInstanceID())
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
    public void Draw(Transform origin)
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

                    var n = Physics.OverlapSphereNonAlloc(point, collisionCheckRadius, hitColliders, collisionMask);
                    bool foreignObstacle = false;
                    for (int p = 0; p < n && !foreignObstacle; p++)
                    {
                        if (hitColliders[p].transform.root.GetInstanceID() != origin.root.GetInstanceID())
                        {
                            foreignObstacle = true;
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
