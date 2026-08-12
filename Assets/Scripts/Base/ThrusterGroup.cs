using UnityEngine;

public class ThrusterGroup : MonoBehaviour
{
    [SerializeField] protected float minScale = 0.1f;
    [SerializeField] protected float maxScale = 1;
    [Tooltip("Multiplier applied to delta time when increasing or decreasing thruster size.")]
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

    protected void OnEnable()
    {
        Transform.localScale = new(1, 1, minScale);
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
