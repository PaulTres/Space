using Effects;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;
public abstract class Ship : Unit
{
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
        if (effectOnDeath != null)
        {
            if (!PoolableEffect.Get(effectOnDeath.PoolKey, out var e)) e = Instantiate(effectOnDeath);
            e.transform.position = Transform.position;
            e.transform.localScale = Transform.localScale;
            e.gameObject.SetActive(true);
        }
        gameObject.SetActive(false);
    }
}
