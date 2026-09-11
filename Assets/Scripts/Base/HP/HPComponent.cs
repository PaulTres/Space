using Effects;
using EventBus;
using System;
using System.Collections.Generic;
using Timers;
using UnityEngine;
using Utilities;

namespace HP
{
    public class HPComponent : MonoBehaviour, IResettable
    {
        #region Fields
        [field: SerializeField] public float MaxHP { get; set; } = 10;
        [Tooltip("The unit will spawn with maxHP*this number.")]
        [Range(0, 1)][SerializeField] protected float startingHPFraction = 1;
        protected Transform tr;
        public Transform Transform
        {
            get
            {
                if (tr == null)
                {
                    tr = transform;
                }
                return tr;
            }
        }
        [SerializeField] protected bool sendEventOnHPChange = false;
        public event Action OnDamageTaken = delegate { };
        [Tooltip("If the ship is not damaged for some time, it will recover health up to this threshold.")]
        [SerializeField] protected float minHP = 0;
        [Tooltip("The time it takes for min hp regain to kick in.")]
        [SerializeField] protected float minHPRegainCooldown = 5;
        [SerializeField] protected float currentHP;
        public float CurrentHP
        {
            get => currentHP;
            set
            {
                value = Mathf.Clamp(value, 0, MaxHP);
                if (currentHP != value)
                {
                    if (sendEventOnHPChange) OnDamageTaken?.Invoke();
                    currentHP = value;
                    UpdateDamageEffects();
                }
            }
        }
        protected CountdownTimer hpRegainTimer;
        protected bool firedEvent = false;
        [SerializeField] protected List<DamageEffect> damageEffects = new();
        protected ImprovedMonoBehaviour improvedMonoBehaviour;
        public ImprovedMonoBehaviour ImprovedMonoBehaviour
        {
            get
            {
                if (improvedMonoBehaviour == null) improvedMonoBehaviour = Transform.root.GetComponent<ImprovedMonoBehaviour>();
                return improvedMonoBehaviour;
            }
        }
        [SerializeField] protected PoolableEffect effectOnDamage;
        [Tooltip("The threshold of damage at which the damage effect will be played.")]
        [SerializeField] protected float damageThreshold = 0.5f;
        [SerializeField] protected float effectScaleFactor = 1;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            if (minHP > 0)
            {
                hpRegainTimer = new(minHPRegainCooldown);
                hpRegainTimer.OnTimerStop += () =>
                {
                    if (currentHP < minHP) currentHP = minHP;
                };
            }
            if (damageEffects.IsEmpty()) damageEffects = new(Transform.root.GetComponentsInChildren<DamageEffect>(true));
        }

        public void PerformReset()
        {
            firedEvent = false;
            MaxHP *= GlobalConfig.GetHPModifier(ImprovedMonoBehaviour.Team);
            CurrentHP = MaxHP * startingHPFraction;
        }

        protected void OnEnable() => PerformReset();

        protected void OnDisable()
        {
            OnDamageTaken = delegate { };
        }

        protected void OnDestroy()
        {
            hpRegainTimer?.Dispose();
        }
        #endregion

        public static bool TakeDamage(Transform transform, DamageInfo damageInfo)
        {
            return EventBus<DamageInfo>.Raise(transform.GetInstanceID(), damageInfo);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (damageInfo.Source != null && damageInfo.Source.Team == transform.root.GetTeam())
            {
                //Debug.Log("Friendly fire");
                return;
            }
            if (ImprovedMonoBehaviour.IsDying) return;
            //Debug.Log($"{this} took {damageInfo.Amount} damage from {damageInfo.Source}.");
            CurrentHP -= damageInfo.Amount;
            if (CurrentHP <= 0 && !firedEvent)
            {
                firedEvent = true;
                ObjectDestroyed @event = new(Transform.GetInstanceID(), damageInfo.Source);
                EventBus<ObjectDestroyed>.Raise(Transform.GetInstanceID(), @event);
                //foreach (var e in damageEffects)
                //{
                //    e.gameObject.SetActive(false);
                //}
                return;
            }
            if (damageInfo.Amount >= damageThreshold) effectOnDamage.SpawnEffect(damageInfo.Position, effectScaleFactor);
            if (currentHP < minHP)
            {
                //begin hp regain timer
                hpRegainTimer.Start();
            }
        }

        protected void OnValidate()
        {
            if (damageEffects.IsEmpty()) damageEffects = new(Transform.GetComponentsInChildren<DamageEffect>(true));
            if (Transform.GetComponentsInChildren<DamageReceiver>() == null) Debug.LogError($"{Transform} has no damage receivers!");
            if (GetComponent<DamageReceiver>() == null)
            {
                Debug.Log($"{this} has no damage receiver, adding damage receiver!");
                gameObject.AddComponent<DamageReceiver>();
            }
        }

        protected void UpdateDamageEffects()
        {
            foreach (var damageEffect in damageEffects)
            {
                damageEffect.gameObject.SetActive(currentHP / MaxHP <
                    damageEffect.DamageThreshold);
            }
        }

    }
}