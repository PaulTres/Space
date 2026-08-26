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
        [System.Serializable]
        public class ArmorModifier
        {
            [field: SerializeField] public Directions Direction { get; protected set; }
            [Tooltip("Incoming damage is multiplied by this modifier.")]
            [field: SerializeField] public float Modifier { get; protected set; } = 1;
        }

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
                    foreach (var damageEffect in damageEffects)
                    {
                        damageEffect.gameObject.SetActive(currentHP / MaxHP <
                            damageEffect.DamageThreshold);
                    }
                }
            }
        }
        protected CountdownTimer hpRegainTimer;
        protected bool firedEvent = false;
        [SerializeField] protected List<ArmorModifier> armorModifiers = new();
        [Tooltip("This transform is used to determine the direction of incoming damage. If not set, the transform of this component will be used.")]
        [SerializeField] protected Transform body;
        protected List<DamageEffect> damageEffects = new();
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
        [SerializeField] protected float effectScaleFactor = 1;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            if (!EventBus<DamageInfo>.AddActions(transform.root.GetInstanceID(), TakeDamage))
            {
                Debug.LogWarning($"{Transform} unable to add DamageInfo actions to event bus binding.");
            }
            if (minHP > 0)
            {
                hpRegainTimer = new(minHPRegainCooldown);
                hpRegainTimer.OnTimerStop += () =>
                {
                    if (currentHP < minHP) currentHP = minHP;
                };
            }
            if (body == null) body = Transform;
            damageEffects = new(Transform.root.GetComponentsInChildren<DamageEffect>(true));
        }

        public void PerformReset()
        {
            firedEvent = false;
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
            EventBus<DamageInfo>.RemoveBinding(transform.root.GetInstanceID());
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
            //Debug.Log(damageInfo.Amount);
            CurrentHP -= CalculateDamage(damageInfo);
            if (CurrentHP <= 0 && !firedEvent)
            {
                firedEvent = true;
                ObjectDestroyed @event = new(Transform.root, damageInfo.Source);
                EventBus<ObjectDestroyed>.Raise(Transform.root.GetInstanceID(), @event);
                foreach (var e in damageEffects)
                {
                    e.gameObject.SetActive(false);
                }
                return;
            }
            effectOnDamage.SpawnEffect(damageInfo.Position, effectScaleFactor);
            if (currentHP < minHP)
            {
                //begin hp regain timer
                hpRegainTimer.Start();
            }
        }

        public float CalculateDamage(DamageInfo damageInfo)
        {
            if (damageInfo.IgnoreArmor || armorModifiers.IsEmpty()) return damageInfo.Amount;
            var direction = body.GetDirection(damageInfo.Position);
            for (int i = 0; i < armorModifiers.Count; i++)
            {
                if (armorModifiers[i].Direction == direction)
                {
                    return damageInfo.Amount * armorModifiers[i].Modifier;
                }
            }
            return damageInfo.Amount;
        }
    }
}