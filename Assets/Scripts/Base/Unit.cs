using AudioSystem;
using Effects;
using EventBus;
using HP;
using HybridBT2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;

[RequireComponent(typeof(HPComponent))]
[RequireComponent(typeof(ParticleSystem))]
public abstract class Unit : MonoBehaviour, IObject
{
    #region Fields
    #region Misc
    [Header("Unit")]
    [Tooltip("This is mostly used for object pooling for the bullets. Never manually modify.")]
    [SerializeField] protected IObject.Type type;
    [Tooltip("If true, when this unit is destroyed all its inflight missiles " +
        "will have their owner set to null.")]
    [SerializeField] protected bool deleteOwnershipOnDeath = true;
    /// <summary>
    /// This is used for quick removal from the unit manager lists. 
    /// DO NOT FUCK WITH THIS!
    /// </summary>
    public int UnitManagerIndex { get; set; }
    [field: SerializeField] public int Team { get; set; }
    [field: SerializeField] public List<Turret> Turrets { get; protected set; } = new();
    /// <summary>
    /// Fires on every tick. Mainly used to update turrets.
    /// </summary>
    protected Action onTick = delegate { };
    /// <summary>
    /// Used to check for missiles in the vicinity of the unit.
    /// </summary>
    [SerializeField] protected float tickInterval = 0.5f;
    protected WaitForSeconds wait;
    protected Coroutine tickCoroutine;
    #endregion

    #region HP
    protected HPComponent hpComponent;
    public float CurrentHP => hpComponent != null ? hpComponent.CurrentHP : 0;
    public float CurrentHPPercentage => hpComponent != null ?
        hpComponent.CurrentHP / hpComponent.MaxHP : 0;
    #endregion

    #region Death
    [Header("Death")]
    [Tooltip("Plays when the ship has finished exploding.")]
    [SerializeField] protected PoolableEffect effectOnDeath;
    [field: SerializeField] public float EffectScaleFactor { get; protected set; } = 1;
    public bool IsDying { get; protected set; }
    /// <summary>
    /// Plays while the unit explodes. Unit will be disabled once the effect is 
    /// finished playing.
    /// </summary>
    protected ParticleSystem effectDuringDeath;
    public ParticleSystem EffectDuringDeath
    {
        get
        {
            if (effectDuringDeath == null)
            {
                effectDuringDeath = GetComponent<ParticleSystem>();
                //effectDuringDeath.loop = false;
            }
            return effectDuringDeath;
        }
    }
    [Tooltip("Plays while the ship is exploding.")]
    [SerializeField] protected SoundData soundDuringDeath;
    #endregion

    #region Scan/Signature
    public float Signature { get; protected set; }
    [field: SerializeField] public float MinimumSignature { get; protected set; }
    [field: SerializeField] public float MaxSignature { get; protected set; }
    [SerializeField] protected float signatureDecreaseRate = 0.1f;
    [SerializeField] protected float signatureDecreaseCooldown = 5f;
    /// <summary>
    /// The time it takes before the signature starts decreasing after it has been increased. 
    /// This is used to prevent the signature from decreasing too quickly after it has been 
    /// increased.
    /// </summary>
    protected float signatureDecreaseDelay;
    [SerializeField] protected float scanRange = 10;
    public float ScanRange
    {
        get
        {
            return IsFleeing ? 0 : scanRange;
        }
    }
    #endregion

    #region Helpers
    public IObject.Type ObjectType => type;
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
    /// <summary>
    /// The unit this is currently targeting.
    /// </summary>
    public virtual IObject Target
    {
        get
        {
            var blackboard = Transform.GetBlackboard();
            if (blackboard == null) return null;
            return blackboard.GetData<Unit>(Blackboard.Keys.Target);
        }
    }
    public bool IsFleeing
    {
        get
        {
            var b = Transform.GetBlackboard();
            if (b == null) return false;
            return b.GetData<bool>(Blackboard.Keys.Fleeing);
        }
    }
    protected Collider[] missileBuffer;
    protected int missileCount;
    #endregion
    #endregion

    #region Setup
    protected virtual void Awake()
    {
        MaxSignature = Mathf.Max(MaxSignature, MinimumSignature);
        Turrets ??= new(GetComponentsInChildren<Turret>());
        wait = new(tickInterval);
        if (hpComponent != null) hpComponent = GetComponent<HPComponent>();
        ComponentRegister<Unit>.Register(Transform, this);
        foreach (var t in Turrets) onTick += t.UpdateTargets;
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetInstanceID(this), Die);
    }

    protected virtual void OnEnable()
    {
        IsDying = false;
        tickCoroutine = StartCoroutine(Tick());
        missileBuffer = new Collider[GlobalConfig.MaxMissileCheckBufferSize];
        Signature = MinimumSignature;
        UnitManager.Register(this);
        this.RegisterForRadar();
        signatureDecreaseDelay = Time.time;
    }

    /// <summary>
    /// Sets the layer of all important objects to a specified layer.
    /// </summary>
    /// <param name="layer"></param>
    public virtual void SetObjectLayer(int layer)
    {
        gameObject.layer = layer;
        foreach (var c in GetComponentsInChildren<Collider>())
        {
            c.gameObject.layer = layer;
        }

    }

    protected virtual void OnDisable()
    {
        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        this.UnregisterForRadar();
        UnitManager.Unregister(this);
        if (IsDying) effectOnDeath.SpawnEffect(Transform.position, EffectScaleFactor);
        missileBuffer = null;
    }

    protected virtual void OnDestroy()
    {
        onTick = null;
        EventBus<ObjectDestroyed>.RemoveBinding(Extensions.GetInstanceID(this));
        ComponentRegister<Unit>.Unregister(transform);
    }
    #endregion

    #region Functionality

    #region Targeting
    /// <summary>
    /// Detect surrounding missiles, fire tick event(turrets), and decrease signature.
    /// </summary>
    /// <returns></returns>
    protected IEnumerator Tick()
    {
        yield return wait;
        GetSurroundingMissiles();
        onTick?.Invoke();
        DecreaseSignature(tickInterval);
    }

    protected virtual void GetSurroundingMissiles()
    {
        missileCount = Physics.OverlapSphereNonAlloc(Transform.position, ScanRange, missileBuffer, GlobalConfig.GetEnemyBulletsCheckMask(gameObject.layer));
    }

    public (int, Collider[]) GetMissiles() => (missileCount, missileBuffer);

    /// <summary>
    /// Returns a list of all units that are on a different Team than this unit.
    /// </summary>
    /// <returns></returns>
    public List<Unit> GetTargets() => UnitManager.GetTargets(Team);
    #endregion

    protected virtual void OnDrawGizmos()
    {
        if (!Application.IsPlaying(this)) Signature = MinimumSignature;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Transform.position, Signature);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Transform.position, ScanRange);
    }

    public void IncreaseSignature(float amount)
    {
        signatureDecreaseDelay = Time.time;
        Signature = Mathf.Min(MaxSignature, Signature + amount);
    }

    protected virtual void DecreaseSignature(float deltaTime)
    {
        if (Time.time - signatureDecreaseDelay < signatureDecreaseCooldown) return;
        Signature = Mathf.Max(MinimumSignature, Signature - signatureDecreaseRate * deltaTime);
    }

    /// <summary>
    /// Fires when an object reaches 0 health.
    /// </summary>
    /// <param name="event"></param>
    public virtual void Die(ObjectDestroyed @event)
    {
        //Debug.Log($"{Transform} with index {UnitManagerIndex} is dead.");
        EventBus<ObjectDestroyed>.RemoveActions(Extensions.GetInstanceID(this), Die);
        UnitManager.Unregister(this);
        EffectDuringDeath.Play();
        IsDying = true;
        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        this.UnregisterForRadar();
        EventBus<UnitDestroyed>.Raise(new(this));
    }
    #endregion
}