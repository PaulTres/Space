using UnityEngine;

public class ThrusterGroup : MonoBehaviour
{
    [SerializeField] protected Vector3 minScale = Vector3.one * .1f;
    [SerializeField] protected Vector3 maxScale = Vector3.one;
    [Tooltip("Multiplier applied to delta time when increasing or decreasing thruster size.")]
    [SerializeField] protected Vector3 scaleFactor = Vector3.one;
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
        Transform.localScale = minScale;
    }

    public void Increase(float dt)
    {
        Transform.localScale = new Vector3(
            Mathf.Clamp(Transform.localScale.x + dt * scaleFactor.x, minScale.x, maxScale.x),
            Mathf.Clamp(Transform.localScale.y + dt * scaleFactor.y, minScale.y, maxScale.y),
            Mathf.Clamp(Transform.localScale.z + dt * scaleFactor.z, minScale.z, maxScale.z));
    }

    public void Decrease(float dt)
    {
        Transform.localScale = new Vector3(
            Mathf.Clamp(Transform.localScale.x - dt * scaleFactor.x, minScale.x, maxScale.x),
            Mathf.Clamp(Transform.localScale.y - dt * scaleFactor.y, minScale.y, maxScale.y),
            Mathf.Clamp(Transform.localScale.z - dt * scaleFactor.z, minScale.z, maxScale.z));
    }
}
