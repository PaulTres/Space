using UnityEngine;
using Utilities;

[RequireComponent(typeof(Rigidbody))]
public abstract class Ship : Unit
{
    #region Fields
    [Header("Ship")]
    [SerializeField] protected float forwardThrust = 1;
    [field: SerializeField] public float RotationSpeed { get; protected set; } = 5;
    [SerializeField] protected float strafeThrust = 1;
    protected ThrusterGroup thrusters;
    protected Rigidbody rb;
    protected Vector3 angularVelocity;
    public override Vector3 AngularVelocity => angularVelocity;
    public override Vector3 LinearVelocity => rb != null ? rb.linearVelocity : Vector3.zero;
    [field: SerializeField] public bool Dampeners { get; protected set; } = true;
    #endregion

    #region Setup
    protected override void Awake()
    {
        base.Awake();
        ComponentRegister<ImprovedMonoBehaviour>.Register(transform, this);
        thrusters = GetComponentInChildren<ThrusterGroup>();
        rb = Transform.GetOrAddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.linearDamping = 0;
        var speedModifier = GlobalConfig.GetUnitSpeedModifier(Team);
        strafeThrust *= speedModifier;
        forwardThrust *= speedModifier;
        RotationSpeed *= speedModifier;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
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
