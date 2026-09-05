using Radar;
using UnityEngine;
using Utilities;

public abstract class ImprovedMonoBehaviour : MonoBehaviour, IRegisterableComponent
{
    [field: SerializeField] public int Team { get; set; } = 1;
    [field: SerializeField] public float MaxVelocityMagnitude { get; protected set; } = 20f;
    public abstract ImprovedMonoBehaviour Target { get; }
    public virtual Vector3 Forward => Transform.forward;
    public virtual Vector3 Right => Transform.right;
    public virtual Vector3 Up => Transform.up;
    public virtual Vector3 LinearVelocity => Vector3.zero;
    public virtual Vector3 AngularVelocity => Vector3.zero;
    Transform tr;
    public Transform Transform
    {
        get
        {
            if (tr == null) tr = transform;
            return tr;
        }
    }
    public Vector3 Position
    {
        get => Transform.position;
        set => Transform.position = value;
    }
    public float CurrentSignature { get; protected set; } = 0;
    [field: SerializeField] public bool IsDying { get; protected set; } = false;

    public void RegisterForRadar()
    {
        var r = PlayerTargetSystem.TryGetInstance();
        if (r != null)
        {
            r.RegisterTarget(this);
        }
    }

    public void UnregisterForRadar()
    {
        var r = PlayerTargetSystem.TryGetInstance();
        if (r != null)
        {
            r.UnregisterTarget(this);
        }
    }
}
