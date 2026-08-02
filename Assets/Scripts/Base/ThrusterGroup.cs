using UnityEngine;

public class ThrusterGroup : MonoBehaviour
{
    [SerializeField] protected float minScale = 0.1f;
    [SerializeField] protected float maxScale = 1;
    [SerializeField] protected float scaleFactor = 1;
    protected Transform tr;
    public Transform Transform
    {
        get
        {
            if (tr == null) tr = transform;
            return tr;
        }
    }

    public void Increase(float dt)
    {
        Transform.localScale = new Vector3(Transform.localScale.x, Transform.localScale.y,
            Mathf.Clamp(Transform.localScale.z + dt * scaleFactor, minScale, maxScale));
    }

    public void Decrease(float dt)
    {
        Transform.localScale = new Vector3(Transform.localScale.x, Transform.localScale.y,
            Mathf.Clamp(Transform.localScale.z - dt * scaleFactor, minScale, maxScale));
    }
}
