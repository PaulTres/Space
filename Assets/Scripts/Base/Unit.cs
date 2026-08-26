using Effects;
using EventBus;
using HP;
using HybridBT2;
using Level;
using System;
using System.Collections;
using System.Collections.Generic;
using Timers;
using UnityEngine;
using Utilities;
using Weapons;


[RequireComponent(typeof(HPComponent))]
[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(AudioSource))]
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
        [Tooltip("Plays when the ship has finished exploding.")]
        public PoolableEffect EffectOnDeath;
        [Tooltip("The scale of effects played for this unit will be multiplied by this number.")]
        [field: SerializeField] public float EffectScaleFactor { get; protected set; } = 1;
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
    /// <summary>
    /// This is used for quick removal from the unit manager lists. 
    /// DO NOT FUCK WITH THIS!
    /// </summary>
    public int UnitManagerIndex { get; set; }
    [Tooltip("The higher this value, the more likely other ships are to attempt to guard this vessel.")]
    [field: SerializeField] public float EscortValue { get; set; } = 0;
    public List<Turret> Turrets { get; protected set; }
    /// <summary>
    /// Fires on every tick. Mainly used to update turrets.
    /// </summary>
    protected Action onTick = delegate { };
    /// <summary>
    /// Plays while the unit explodes. UnitProperty will be disabled once the effect is 
    /// finished playing.
    /// </summary>
    protected ParticleSystem effectDuringDeath;
    protected AudioSource audioSource;
    [Tooltip("Telemetry for missiles will be calculated from here. If not set will use the component's transform.")]
    [field: SerializeField] public Transform TelemetryOrigin { get; protected set; }
    [Tooltip("These objects will have their layer changed to better account for collisions.")]
    [SerializeField] protected List<GameObject> collisionCheckers;
    [Tooltip("The list of all objects whose layer needs to be changed for taking damage on spawn. Should contain only colliders.")]
    [SerializeField] protected List<GameObject> damageTakingObjects;
    [Tooltip("If true, the spawn effect will play when the unit is enabled.")]
    [field: SerializeField] public bool PlaySpawnEffect { get; set; } = true;
    #endregion

    [SerializeField] protected UnitParameters unitParameters;

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
    protected CountdownTimer envDamageTimer;
    protected int safeZoneCount = 0;
    /// <summary>
    /// How many safe zones are covering this unit. If zero and in a dangerous environment, this unit will take damage.
    /// </summary>
    public int SafeZoneCount
    {
        get
        {
            return safeZoneCount;
        }
        set
        {
            value = Mathf.Max(0, value);
            if (safeZoneCount != value)
            {
                safeZoneCount = value;
                if (safeZoneCount > 0)
                {
                    envDamageTimer?.Stop();
                }
                else
                {
                    var i = EnvironmentDamage.TryGetInstance();
                    if (i == null) return;
                    if (envDamageTimer == null)
                    {
                        envDamageTimer = new(i.Interval);
                        envDamageTimer.OnTimerStop += DealEnvironmentDamage;
                    }
                    envDamageTimer.Start();
                }
            }
        }
    }

    public float SpawnCheckRadius => unitParameters.SpawnCheckRadius;
    public float MoraleWeight => unitParameters.MoraleWeight;
    public float MaxSignature => unitParameters.MaxSignature;
    protected WaitForSeconds wait;
    protected Coroutine tickCoroutine;
    protected float signatureDecreaseDelay;
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
    public AudioSource AudioSource
    {
        get
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                Debug.Assert(audioSource.playOnAwake == false);
            }
            return audioSource;
        }
    }
    public float EffectScaleFactor => unitParameters.EffectScaleFactor;
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
        Debug.Assert(!damageTakingObjects.IsEmpty(), $"{Transform} has no critical objects set!");
        if (TelemetryOrigin == null) TelemetryOrigin = Transform;
        Turrets = new(GetComponentsInChildren<Turret>());
        wait = new(unitParameters.TickInterval);
        ComponentRegister<Unit>.Register(Transform, this);
        foreach (var t in Turrets) onTick += t.UpdateTargets;
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetInstanceID(this), Die);
        var main = EffectDuringDeath.main;
        main.stopAction = ParticleSystemStopAction.Disable;
    }

    protected virtual void OnEnable()
    {
        IsDying = false;
        missileBuffer = new Collider[GlobalConfig.MaxMissileCheckBufferSize];
        missileCount = 0;
        tickCoroutine = StartCoroutine(Tick());
        CurrentSignature = unitParameters.MinimumSignature;
        UnitManager.Register(this);
        signatureDecreaseDelay = Time.time;
        if (PlaySpawnEffect && unitParameters.EffectOnSpawn != null)
        {
            unitParameters.EffectOnSpawn.SpawnEffect(Position, EffectScaleFactor);
        }
    }

    /// <summary>
    /// Sets the layers of all important objects.
    /// </summary>
    /// <param name="layer"></param>
    public virtual void SetObjectLayer()
    {
        Debug.Assert(damageTakingObjects.Count > 0, $"{Transform} has no objects for taking damage!");
        Debug.Assert(collisionCheckers.Count > 0, $"{Transform} has no objects for handling collisions!");
        foreach (var c in damageTakingObjects) c.layer = GlobalConfig.GetShipLayer(Team);
        foreach (var c in collisionCheckers) c.layer = GlobalConfig.GetCollisionLayer(Team);
    }

    protected virtual void OnDisable()
    {
        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        UnitManager.Unregister(this);
        //only generate the effect when the scene is loaded (not unloading) and the unit is in the process of dying
        if (IsDying && !effectDuringDeath.isPlaying && gameObject.scene.isLoaded)
        {
            unitParameters.EffectOnDeath.SpawnEffect(Transform.position, unitParameters.EffectScaleFactor);
        }
        AudioSource.Stop();
        missileBuffer = null;
        AudioSource.Stop();
        envDamageTimer?.Stop();
    }

    protected virtual void OnDestroy()
    {
        onTick = null;
        envDamageTimer?.Dispose();
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
        //Debug.Log(missileCount);
    }

    public (int, Collider[]) GetMissiles() => (missileCount, missileBuffer);

    /// <summary>
    /// Returns a list of all units that are on a different Team than this unit.
    /// </summary>
    /// <returns></returns>
    public List<Unit> GetTargets() => UnitManager.GetTargets(Team);
    #endregion

    public List<Unit> GetAllies() => UnitManager.GetShips(Team);

    public void IncreaseSignature(float amount)
    {
        signatureDecreaseDelay = Time.time;
        CurrentSignature = Mathf.Min(unitParameters.MaxSignature, CurrentSignature + amount);
    }

    protected virtual void DecreaseSignature(float deltaTime)
    {
        if (Time.time - signatureDecreaseDelay < unitParameters.SignatureDecreaseCooldown) return;
        CurrentSignature = Mathf.Max(unitParameters.MinimumSignature,
            CurrentSignature - unitParameters.SignatureDecreaseRate * deltaTime);
    }

    /// <summary>
    /// Fires when an object reaches 0 health.
    /// </summary>
    /// <param name="event"></param>
    public virtual void Die(ObjectDestroyed @event)
    {
        if (IsDying) return;
        //Debug.Log($"{Transform} with index {UnitManagerIndex} is dead.");
        GameManager.UnitsDestroyed.Add(@event);
        EventBus<ObjectDestroyed>.RemoveActions(Extensions.GetInstanceID(this), Die);
        EffectDuringDeath.Play();
        AudioSource.Play();
        IsDying = true;
        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        UnitManager.Unregister(this);
        EventBus<UnitDestroyed>.Raise(new(this));
    }

    public void DealEnvironmentDamage()
    {
        if (safeZoneCount > 0 || !this.Active()) return;
        var u = EnvironmentDamage.TryGetInstance();
        if (u != null)
        {
            u.DealDamage(this);
            envDamageTimer.Start();
        }
    }
    #endregion

    #region Debugging
    protected virtual void OnDrawGizmos()
    {
        if (!Application.IsPlaying(this)) CurrentSignature = unitParameters.MinimumSignature;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Transform.position, CurrentSignature);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Transform.position, ScanRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Transform.position, SpawnCheckRadius);
    }

    protected virtual void OnValidate()
    {
        Debug.Assert(!AudioSource.playOnAwake, $"{Transform}'s audio source is set to play in Awake!");
    }
    #endregion
}