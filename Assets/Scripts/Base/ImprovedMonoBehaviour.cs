using Radar;
using UnityEngine;
using Utilities;

public abstract class ImprovedMonoBehaviour : MonoBehaviour, IRegisterableComponent
{
    [field: SerializeField] public int Team { get; set; }
    public abstract ImprovedMonoBehaviour Target { get; }
    public virtual Vector3 Forward => Transform.forward;
    public virtual Vector3 Right => Transform.forward;
    public virtual Vector3 Up => Transform.forward;
    public virtual Vector3 LinearVelocity => Vector3.zero;
    public virtual Vector3 AngularVelocity => Vector3.zero;
    protected Transform tr;
    public Transform Transform
    {
        get
        {
            if (tr == null) tr = transform;
            return tr;
        }
    }
    public Vector3 Position => Transform.position;
    public float CurrentSignature { get; protected set; } = 0;
    public bool IsDying { get; protected set; } = false;

    protected void RegisterForRadar()
    {
        var r = PlayerTargetSystem.TryGetInstance();
        if (r != null)
        {
            r.RegisterTarget(this);
        }
    }

    protected void UnregisterForRadar()
    {
        var r = PlayerTargetSystem.TryGetInstance();
        if (r != null)
        {
            r.UnregisterTarget(this);
        }
    }
}
