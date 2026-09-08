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
[RequireComponent(typeof(UnitDeathEffects))]
public abstract class Unit : ImprovedMonoBehaviour
{
    [System.Serializable]
    public class UnitParameters
    {
        [Tooltip("How much morale damage will allies suffer when this unit is destroyed.")]
        public float MoraleWeight = 10;
        public float TickInterval = 0.2f;
        [Tooltip("Plays when the ship is spawned in.")]
        public PoolableEffect EffectOnSpawn;
        [field: SerializeField] public float MinimumSignature { get; protected set; }
        [field: SerializeField] public float MaxSignature { get; protected set; }
        public float SignatureDecreaseRate = 0.1f;
        [Tooltip("The time it takes before the signature starts decreasing after it has been increased. This is used to prevent the signature from decreasing too quickly after it has been increased.")]
        public float SignatureDecreaseCooldown = 5f;
        public float ScanRange = 10;
        [Tooltip("On spawn, the unit will check for obstructions in a sphere of this radius.")]
        public float SpawnCheckRadius = 5;
    }

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

    [SerializeField] protected UnitParameters unitParameters;
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
    public UnitDeathEffects UnitDeathEffects
    {
        get
        {
            if (deathEffects == null) deathEffects = GetComponentInChildren<UnitDeathEffects>();
            return deathEffects;
        }
    }
    public float SpawnCheckRadius => unitParameters.SpawnCheckRadius;
    public float MoraleWeight => unitParameters.MoraleWeight;
    public float MaxSignature => unitParameters.MaxSignature;
    protected WaitForSeconds wait;
    protected Coroutine tickCoroutine;
    protected float signatureDecreaseDelay;
    public float ScanRange
    {
        get
        {
            return IsFleeing ? 0 : unitParameters.ScanRange;
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
    protected int missileCount = 0;
    #endregion
    #endregion

    #region Setup/Cleanup
    protected virtual void Awake()
    {
        //Debug.Assert(!damageTakingObjects.IsEmpty(), $"{Transform} has no critical objects set!");
        if (TelemetryOrigin == null) TelemetryOrigin = Transform;
        Turrets = new(GetComponentsInChildren<Turret>());
        wait = new(unitParameters.TickInterval);
        ComponentRegister<Unit>.Register(Transform.root, this);
        foreach (var t in Turrets) onTick += t.UpdateTargets;
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetTransformInstanceID(this), Die);
    }

    protected virtual void OnEnable()
    {
        IsDying = false;
        missileBuffer = new Collider[GlobalConfig.MaxMissileCheckBufferSize];
        missileCount = 0;
        tickCoroutine = StartCoroutine(Tick());
        currentSignature = unitParameters.MinimumSignature;
        UnitManager.Register(this);
        signatureDecreaseDelay = Time.time;
        if (PlaySpawnEffect && unitParameters.EffectOnSpawn != null)
        {
            unitParameters.EffectOnSpawn.SpawnEffect(Position, EffectScaleFactor);
        }
    }

    protected void OnValidate()
    {
        if (damageTakingObjects.IsEmpty()) damageTakingObjects = new(GetComponentsInChildren<DamageReceiver>());
    }

    /// <summary>
    /// Sets the layers of all important objects.
    /// </summary>
    /// <param name="layer"></param>
    public override void SetObjectLayer()
    {
        Debug.Assert(damageTakingObjects.Count > 0, $"{Transform} has no objects for taking damage!");
        Debug.Assert(collisionCheckers.Count > 0, $"{Transform} has no objects for handling collisions!");
        foreach (var c in damageTakingObjects) c.gameObject.layer = GlobalConfig.GetShipLayer(Team);
        foreach (var c in collisionCheckers) c.gameObject.layer = GlobalConfig.GetCollisionLayer(Team);
    }

    protected virtual void OnDisable()
    {
        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        UnitManager.Unregister(this);
        missileBuffer = null;
    }

    protected virtual void OnDestroy()
    {
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
            DecreaseSignature(unitParameters.TickInterval);
        }
    }

    protected virtual void GetSurroundingMissiles()
    {
        missileCount = Physics.OverlapSphereNonAlloc(Position, ScanRange,
            missileBuffer, GlobalConfig.GetEnemyBulletsCheckMask(Team));
        //Debug.Log($"{Transform} missile Count: {missileCount}");
    }

    public (int, Collider[]) GetMissiles() => (missileCount, missileBuffer);

    /// <summary>
    /// Returns a list of all units that are on a different Team than this unit.
    /// </summary>
    /// <returns></returns>
    public List<ImprovedMonoBehaviour> GetTargets() => UnitManager.GetTargets(Team);
    #endregion

    public List<ImprovedMonoBehaviour> GetAllies() => UnitManager.GetShips(Team);

    public void IncreaseSignature(float amount)
    {
        signatureDecreaseDelay = Time.time;
        currentSignature = Mathf.Min(unitParameters.MaxSignature, CurrentSignature + amount);
    }

    protected virtual void DecreaseSignature(float deltaTime)
    {
        if (Time.time - signatureDecreaseDelay < unitParameters.SignatureDecreaseCooldown) return;
        currentSignature = Mathf.Max(unitParameters.MinimumSignature,
            CurrentSignature - unitParameters.SignatureDecreaseRate * deltaTime);
    }

    /// <summary>
    /// Fires when an object reaches 0 health.
    /// </summary>
    /// <param name="event"></param>
    public virtual void Die(ObjectDestroyed @event)
    {
        if (IsDying) return;
        IsDying = true;
        //Debug.Log($"{Transform} is dead.");
        UnitDestroyed unitDestroyed = new(this, @event.Killer);
        GameManager.UnitsDestroyed.Add(unitDestroyed);
        EventBus<ObjectDestroyed>.RemoveActions(Extensions.GetTransformInstanceID(this), Die);
        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        UnitManager.Unregister(this);
        //Debug.Log(unitDestroyed);
        EventBus<UnitDestroyed>.Raise(unitDestroyed);
        UnitDeathEffects.PlayDeathEffects();
    }
    #endregion

    #region Debugging
    protected virtual void OnDrawGizmos()
    {
        if (!Application.IsPlaying(this)) currentSignature = unitParameters.MinimumSignature;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Transform.position, CurrentSignature);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Transform.position, ScanRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Transform.position, SpawnCheckRadius);
    }
    #endregion
}