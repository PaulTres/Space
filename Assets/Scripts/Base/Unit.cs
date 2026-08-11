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
[RequireComponent(typeof(AudioSource))]
public abstract class Unit : ImprovedMonoBehaviour
{
    public enum Type
    {
        Skiff,
        Frigate,
        Cruiser
    }
    [System.Serializable]
    public class UnitParameters
    {
        [field: SerializeField] public Type UnitType { get; protected set; }
        public float TickInterval = 0.2f;
        public PoolableEffect EffectOnSpawn;
        [Tooltip("Plays when the ship has finished exploding.")]
        public PoolableEffect EffectOnDeath;
        [field: SerializeField] public float EffectScaleFactor { get; protected set; } = 1;
        [field: SerializeField] public float MinimumSignature { get; protected set; }
        [field: SerializeField] public float MaxSignature { get; protected set; }
        public float SignatureDecreaseRate = 0.1f;
        [Tooltip("The time it takes before the signature starts decreasing after it has been increased. This is used to prevent the signature from decreasing too quickly after it has been increased.")]
        public float SignatureDecreaseCooldown = 5f;
        public float ScanRange = 10;
    }

    #region Fields
    #region Misc
    [Header("Unit")]
    /// <summary>
    /// This is used for quick removal from the unit manager lists. 
    /// DO NOT FUCK WITH THIS!
    /// </summary>
    public int UnitManagerIndex { get; set; }
    [Tooltip("If true, other friendly units will attemt to remain near this unit.")]
    [field: SerializeField] public bool NeedsEscort { get; set; } = false;
    public List<Turret> Turrets { get; protected set; }
    /// <summary>
    /// Fires on every tick. Mainly used to update turrets.
    /// </summary>
    protected Action onTick = delegate { };
    /// <summary>
    /// Plays while the unit explodes. Unit will be disabled once the effect is 
    /// finished playing.
    /// </summary>
    protected ParticleSystem effectDuringDeath;
    protected AudioSource audioSource;
    [field: SerializeField] public Transform TelemetryOrigin { get; protected set; }
    [Tooltip("The list of all objects whose layer needs to be changed on spawn.")]
    [SerializeField] protected List<GameObject> criticalObjects;
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
    public Type UnitType => unitParameters.UnitType;
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
        Debug.Assert(!criticalObjects.IsEmpty());
        if (TelemetryOrigin == null) TelemetryOrigin = Transform;
        Turrets = new(GetComponentsInChildren<Turret>());
        wait = new(unitParameters.TickInterval);
        ComponentRegister<Unit>.Register(Transform, this);
        foreach (var t in Turrets) onTick += t.UpdateTargets;
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetInstanceID(this), Die);
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
        if (unitParameters.EffectOnDeath != null)
        {
            unitParameters.EffectOnDeath.SpawnEffect(Position, EffectScaleFactor);
        }
    }

    /// <summary>
    /// Sets the layer of all important objects to a specified layer.
    /// </summary>
    /// <param name="layer"></param>
    public virtual void SetObjectLayer(int layer)
    {
        foreach (var c in criticalObjects) c.layer = layer;
    }

    protected virtual void OnDisable()
    {
        if (tickCoroutine != null) StopCoroutine(tickCoroutine);
        UnitManager.Unregister(this);
        if (IsDying && !effectDuringDeath.isPlaying)
        {
            unitParameters.EffectOnDeath.SpawnEffect(Transform.position, unitParameters.EffectScaleFactor);
        }
        missileBuffer = null;
        AudioSource.Stop();
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

    public void ChangeTeam(int team)
    {
        if (Team != team)
        {
            Team = team;
            UnitManager.Unregister(this);
            UnitManager.Register(this);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        if (!Application.IsPlaying(this)) CurrentSignature = unitParameters.MinimumSignature;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Transform.position, CurrentSignature);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Transform.position, ScanRange);
    }

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
    #endregion
}