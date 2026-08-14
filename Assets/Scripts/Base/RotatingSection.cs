using UnityEngine;
using Utilities;

public class RotatingSection : MonoBehaviour
{
    [SerializeField] protected float rotationSpeed = 20;
    protected Transform tr;
    public Transform Transform
    {
        get
        {
            if (tr == null) tr = transform;
            return tr;
        }
    }

    protected void OnEnable()
    {
        GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformFixedUpdate);
    }

    protected void OnDisable()
    {
        var u = GlobalUpdater.TryGetInstance();
        if (u != null) u.UnregisterFixedUpdate(PerformFixedUpdate);
    }

    protected void PerformFixedUpdate(float deltaTime)
    {
        Transform.Rotate(Vector3.forward, rotationSpeed * deltaTime);
    }
}
