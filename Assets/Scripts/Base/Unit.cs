using EventBus;
using HP;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;

public abstract class Unit : MonoBehaviour, IObject
{
    #region Fields
    [Header("Unit")]
    [Tooltip("If true, when this unit is destroyed all its inflight missiles " +
        "will have their owner set to null.")]
    [SerializeField] protected bool deleteOwnershipOnDeath = true;
    public int UnitManagerIndex { get; set; }
    public int Team { get; set; }
    public virtual Vector3 Forward => Transform.forward;
    public virtual Vector3 Right => Transform.forward;
    public virtual Vector3 Up => Transform.forward;
    public virtual Vector3 LinearVelocity => Vector3.zero;
    public virtual Vector3 AngularVelocity => Vector3.zero;
    public float Signature => signature;
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
    protected float signature;
    [SerializeField] protected float defaultSignature;
    [SerializeField] protected float maxSignature;
    [SerializeField] protected float signatureDecreaseRate = 0.1f;
    [SerializeField] protected float signatureDecreaseCooldown = 5f;
    protected float signatureIncreaseTime;
    [field: SerializeField] public float ScanRange { get; protected set; }
    protected HPComponent hpComponent;
    public float CurrentHP => hpComponent != null ? hpComponent.CurrentHP : 0;
    public float CurrentHPPercentage => hpComponent != null ?
        hpComponent.CurrentHP / hpComponent.MaxHP : 0;
    [field: SerializeField] public List<Turret> Turrets { get; protected set; } = new();
    protected Action onTick = delegate { };
    protected Collider[] buffer;
    protected int missileCount;
    [SerializeField] protected float tickInterval = 0.5f;
    protected WaitForSeconds wait;
    protected Coroutine coroutine;
    #endregion

    #region Setup
    protected virtual void Awake()
    {
        maxSignature = Mathf.Max(maxSignature, defaultSignature);
        Turrets ??= new(GetComponentsInChildren<Turret>());
        wait = new(tickInterval);
        if (hpComponent != null) hpComponent = GetComponent<HPComponent>();
        ComponentRegister<Unit>.Register(Transform, this);
        foreach (var t in Turrets) onTick += t.UpdateTargets;
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetInstanceID(this), Die);
    }

    protected virtual void OnEnable()
    {
        coroutine = StartCoroutine(Tick());
        buffer = new Collider[GlobalConfig.MaxMissileCheckBufferSize];
        signature = defaultSignature;
        UnitManager.Register(this);
        this.RegisterForRadar();
        signatureIncreaseTime = Time.time;
    }

    protected virtual void OnDisable()
    {
        if (coroutine != null) StopCoroutine(coroutine);
        UnitManager.Unregister(this);
        this.UnregisterForRadar();
        buffer = null;
    }

    protected virtual void OnDestroy()
    {
        onTick = null;
        EventBus<ObjectDestroyed>.RemoveBinding(Extensions.GetInstanceID(this));
        ComponentRegister<Unit>.Unregister(transform);
    }
    #endregion

    #region Functionality
    public abstract void Die(ObjectDestroyed @event);

    protected IEnumerator Tick()
    {
        yield return wait;
        GetSurroundingMissiles();
        onTick?.Invoke();
        DecreaseSignature(tickInterval);
    }

    public void IncreaseSignature(float amount)
    {
        signatureIncreaseTime = Time.time;
        signature = Mathf.Min(maxSignature, signature + amount);
    }

    protected virtual void DecreaseSignature(float deltaTime)
    {
        if (Time.time - signatureIncreaseTime < signatureDecreaseCooldown) return;
        signature = Mathf.Max(defaultSignature, signature - signatureDecreaseRate * deltaTime);
    }

    protected virtual void GetSurroundingMissiles()
    {
        missileCount = Physics.OverlapSphereNonAlloc(Transform.position, ScanRange, buffer, GlobalConfig.GetEnemyBulletCheckMask(gameObject.layer));
    }

    public (int, Collider[]) GetMissiles() => (missileCount, buffer);

    public List<Unit> GetTargets() => UnitManager.GetTargets(Team);
    #endregion
}