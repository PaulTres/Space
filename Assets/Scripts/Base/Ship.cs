using Unity.VisualScripting;
using UnityEngine;
using Utilities;

public abstract class Ship : Unit
{
    #region Fields
    [Header("Ship")]
    [field: SerializeField] public float MaxVelocityMagnitude { get; protected set; } = 5f;
    [SerializeField] protected float forwardThrust = 1;
    [field: SerializeField] public float RotationSpeed { get; protected set; } = 5;
    [SerializeField] protected float strafeThrust = 1;
    protected ThrusterGroup thrusters;
    protected Rigidbody rb;
    protected Vector3 angularVelocity;
    public override Vector3 AngularVelocity => angularVelocity;
    public override Vector3 LinearVelocity => rb != null ? rb.linearVelocity : Vector3.zero;
    public bool Dampeners { get; protected set; }
    #endregion

    #region Setup
    protected override void Awake()
    {
        base.Awake();
        ComponentRegister<IObject>.Register(transform, this);
        thrusters = GetComponentInChildren<ThrusterGroup>();
        rb = gameObject.GetOrAddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.linearDamping = 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Dampeners = true;
        angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
    }
    #endregion

    public override void Die(ObjectDestroyed @event)
    {
        Dampeners = false;
        base.Die(@event);
    }
}
