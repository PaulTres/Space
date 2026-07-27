using EventBus;
using System;
using Timers;
using UnityEngine;
using Utilities;

namespace HP
{
    public class HPComponent : MonoBehaviour, IResettable
    {
        [SerializeField] protected float maxHP = 10;
        public float MaxHP
        {
            get => maxHP;
            set
            {
                if (value <= 0)
                {
                    Debug.LogError($"MaxHP <= 0 for {transform}. Setting to 1.");
                    value = 1;
                }
                maxHP = value;
            }
        }
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
        protected float currentHP;
        public float CurrentHP
        {
            get => currentHP;
            set
            {
                value = Mathf.Clamp(value, 0, MaxHP);
                if (currentHP != value)
                {
                    if (sendEventOnHPChange)
                    {
                        OnDamageTaken?.Invoke();
                        EventBus<ObjectHPChanged>.Raise(Transform.GetInstanceID(),
                            new ObjectHPChanged(CurrentHP,
                            CurrentHP / (MaxHP + float.Epsilon)));
                    }
                    currentHP = value;
                }
            }
        }
        protected CountdownTimer hpRegainTimer;

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
        }

        public void PerformReset()
        {
            CurrentHP = MaxHP;
        }

        protected void OnEnable()
        {
            PerformReset();
        }

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
            if (damageInfo.Source.GetTeam(throwOnNullTransform: false) == transform.root.GetTeam())
            {
                //Debug.Log("Friendly fire");
                return;
            }
            //Debug.Log(damageInfo.Amount);
            CurrentHP -= damageInfo.Amount;
            if (CurrentHP <= 0)
            {
                ObjectDestroyed @event = new(Transform, damageInfo.Source);
                GameManager.UnitsDestroyed.Add(@event);
                EventBus<ObjectDestroyed>.Raise(Transform.GetInstanceID(), @event);
            }
            else if (currentHP < minHP)
            {
                //begin hp regain timer
                hpRegainTimer.Start();
            }
        }
    }
}