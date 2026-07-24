using Effects;
using EventBus;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;
public abstract class Ship : Unit
{
    #region Fields
    [Header("Ship")]
    [SerializeField] protected PoolableEffect effectOnDeath;
    [field: SerializeField] public float MaxVelocityMagnitude { get; protected set; } = 5f;
    [SerializeField] protected float forwardThrust = 1;
    [field: SerializeField] public float RotationSpeed { get; protected set; } = 5;
    [SerializeField] protected float strafeThrust = 1;
    protected Rigidbody rb;
    protected Vector3 angularVelocity;
    public override Vector3 AngularVelocity => angularVelocity;
    public override Vector3 LinearVelocity => rb != null ? rb.linearVelocity : Vector3.zero;
    #endregion

    #region Setup
    protected override void Awake()
    {
        base.Awake();
        ComponentRegister<IObject>.Register(transform, this);
        rb = gameObject.GetOrAddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearDamping = 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
    }

    protected override void OnDestroy()
    {
        base.Awake();
        ComponentRegister<IObject>.Unregister(transform);
    }
    #endregion

    public override void Die(ObjectDestroyed @event)
    {
        //Debug.Log($"{Transform} with index {UnitManagerIndex} is dead.");
        UnitManager.Unregister(this);
        effectOnDeath.SpawnEffect(Transform.position, EffectScaleFactor);
        EventBus<UnitDestroyed>.Raise(new(this));
        gameObject.SetActive(false);
    }
}
