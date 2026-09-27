using Effects;
using EventBus;
using HP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;

[RequireComponent(typeof(HPComponent))]
[RequireComponent(typeof(UnitDeathEffects))]
[RequireComponent(typeof(ShipParameters))]
public abstract class Unit : ImprovedMonoBehaviour
{
    #region Fields
    #region Misc
    [Header("Unit")]
    public List<Turret> Turrets { get; protected set; }
    /// <summary>
    /// Fires on every tick. Mainly used to update turrets.
    /// </summary>
    protected Action onTick = delegate { };
    [Tooltip("Telemetry for missiles will be calculated from here. If not set will use the component's transform.")]
    [field: SerializeField] public Transform TelemetryOrigin { get; protected set; }
    [Tooltip("These objects will have their layer changed to better account for collisions.")]
    [SerializeField] protected List<Collider> collisionCheckers;
    [Tooltip("The list of all objects whose layer needs to be changed for taking damage on spawn. Should contain only colliders.")]
    [SerializeField] protected List<DamageReceiver> damageTakingObjects;
    [Tooltip("If true, the spawn effect will play when the unit is enabled.")]
    [field: SerializeField] public bool PlaySpawnEffect { get; set; } = true;
    protected float currentSignature = 0;
    public override float CurrentSignature => currentSignature;

    #endregion

    [field: SerializeField] public ShipParameters Parameters { get; protected set; }
    protected UnitDeathEffects deathEffects;

    #region HP
    protected HPComponent hpComponent;
    public HPComponent HPComponent
    {
        get
        {
            if (hpComponent == null) hpComponent = GetComponent<HPComponent>();
            return hpComponent;
        }
    }
    public float MaxHP => HPComponent != null ? HPComponent.MaxHP : 0;
    public float CurrentHP => HPComponent != null ? HPComponent.CurrentHP : 0;
    public float CurrentHPPercentage => HPComponent != null ? HPComponent.CurrentHP / HPComponent.MaxHP : 0;
    #endregion

    #region Helpers
    public override float EscortValue => Parameters.EscortValue;
    public override float EffectScaleFactor => Parameters.EffectScaleFactor;
    public PoolableEffect SpawnEffect => Parameters.EffectOnSpawn;
    public UnitDeathEffects UnitDeathEffects
    {
        get
        {
            if (deathEffects == null) deathEffects = GetComponentInChildren<UnitDeathEffects>();
            return deathEffects;
        }
    }
    public float SpawnCheckRadius => Parameters.SpawnCheckRadius;
    public override int MoraleWeight => Parameters.MoraleWeight;
    public float MaximumSignature => Parameters.MaximumSignature;
    public float MinimumSignature => Parameters.MinimumSignature;
    protected WaitForSeconds wait;
    protected Coroutine tickCoroutine;
    protected float signatureDecreaseDelay;
    public override float ScanRange
    {
        get
        {
            return IsFleeing ? 0 : Parameters.ScanRange;
        }
    }
    public float CurrentMorale
    {
        get
        {
            return UnitManager.GetMorale(Team);
        }
    }
    protected Collider[] missileBuffer;
    protected int missileCount = 0;
    #endregion
    #endregion

    #region Setup/Cleanup
    protected override void Awake()
    {
        base.Awake();
        if (TelemetryOrigin == null) TelemetryOrigin = Transform;
        Turrets = new(GetComponentsInChildren<Turret>());
        wait = new(Parameters.TickInterval);
        ComponentRegister<Unit>.Register(Transform.root, this);
        foreach (var t in Turrets) onTick += t.UpdateTargets;
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetTransformInstanceID(this), Die);
        MaxVelocityMagnitude *= GlobalConfig.GetUnitSpeedModifier(Team);
        var signatureModifier = GlobalConfig.GetSignatureModifier(Team);
        Parameters.MaximumSignature *= signatureModifier;
        Parameters.MinimumSignature *= signatureModifier;
        Parameters.ScanRange *= GlobalConfig.GetScanRangeModifier(Team);
    }

    protected virtual void OnEnable()
    {
        IsDying = false;
        missileBuffer = new Collider[GlobalConfig.MaxMissileCheckBufferSize];
        missileCount = 0;
        tickCoroutine = StartCoroutine(Tick());
        currentSignature = MinimumSignature;
        UnitManager.Register(this);
        signatureDecreaseDelay = Time.time;
        if (PlaySpawnEffect && Parameters.EffectOnSpawn != null)
        {
            Parameters.EffectOnSpawn.SpawnEffect(Position, EffectScaleFactor);
        }
    }

    protected void OnValidate()
    {
        if (damageTakingObjects.IsEmpty()) damageTakingObjects = new(GetComponentsInChildren<DamageReceiver>());
        Debug.Assert(damageTakingObjects.Count > 0, $"{Transform} has no objects for taking damage!");
        Debug.Assert(!collisionCheckers.IsEmpty(), $"{Transform} has no objects for handling collisions!");
        if (Parameters == null)
        {
            Parameters = GetComponent<ShipParameters>();
            MaxVelocityMagnitude = Parameters.MaxVelocityMagnitude;
        }
    }

    /// <summary>
    /// Sets the layers of all important objects.
    /// </summary>
    /// <param name="layer"></param>
    public override void SetObjectLayer()
    {
        foreach (var c in damageTakingObjects) c.gameObject.layer = GlobalConfig.GetShipLayer(Team);
        foreach (var c in collisionCheckers) c.gameObject.layer = GlobalConfig.GetCollisionLayer(Team);
    }

    protected virtual void OnDisable()
    {
        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        UnitManager.Unregister(this);
        missileBuffer = null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onTick = null;
        EventBus<ObjectDestroyed>.RemoveBinding(Extensions.GetTransformInstanceID(this));
        ComponentRegister<Unit>.Unregister(Transform.root);
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
        while (true)
        {
            yield return wait;
            GetSurroundingMissiles();
            onTick?.Invoke();
            DecreaseSignature(Parameters.TickInterval);
        }
    }

    protected virtual void GetSurroundingMissiles()
    {
        missileCount = Physics.OverlapSphereNonAlloc(Position, ScanRange,
            missileBuffer, GlobalConfig.GetEnemyBulletsCheckMask(Team));
    }

    public (int, Collider[]) GetMissiles() => (missileCount, missileBuffer);

    /// <summary>
    /// Returns a list of all units that are on a different team than this unit.
    /// </summary>
    /// <returns></returns>
    public List<ImprovedMonoBehaviour> GetTargets() => UnitManager.GetTargets(Team);
    #endregion

    public List<ImprovedMonoBehaviour> GetAllies() => UnitManager.GetShips(Team);

    public void IncreaseSignature(float amount)
    {
        signatureDecreaseDelay = Time.time;
        currentSignature = Mathf.Min(MaximumSignature, CurrentSignature + amount);
    }

    protected virtual void DecreaseSignature(float deltaTime)
    {
        if (Time.time - signatureDecreaseDelay < Parameters.SignatureDecreaseCooldown) return;
        currentSignature = Mathf.Max(MinimumSignature,
            CurrentSignature - Parameters.SignatureDecreaseRate * deltaTime);
    }

    /// <summary>
    /// Fires when an object reaches 0 health.
    /// </summary>
    /// <param name="event"></param>
    public virtual void Die(ObjectDestroyed @event)
    {
        if (IsDying) return;
        if (@event.Killer == null && IsFleeing) return;
        IsDying = true;
        UnitDestroyed unitDestroyed = new(this, @event.Killer);
        GameManager.UnitsDestroyed.Add(unitDestroyed);
        EventBus<ObjectDestroyed>.RemoveActions(Extensions.GetTransformInstanceID(this), Die);
        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        UnitManager.Unregister(this);
        EventBus<UnitDestroyed>.Raise(unitDestroyed);
        UnitDeathEffects.PlayDeathEffects();
    }
    #endregion

    #region Debugging
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Transform.position, MinimumSignature);
        Gizmos.DrawWireSphere(Transform.position, MaximumSignature);
        Gizmos.DrawWireSphere(Transform.position, CurrentSignature);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Transform.position, ScanRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Transform.position, SpawnCheckRadius);
        if (Target.Active())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Transform.position, Target.Position);
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(Transform.position, EffectScaleFactor);
    }
    #endregion
}