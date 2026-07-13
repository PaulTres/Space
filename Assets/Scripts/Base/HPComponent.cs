using EventBus;
using System;
using UnityEngine;
using Utilities;
namespace HP
{
    public class HPComponent : MonoBehaviour, IResettable
    {
        [SerializeField] protected float maxHP;
        public float CurrentHP { get; protected set; }
        protected Transform tr;
        [SerializeField] protected bool sendEventOnDamage = false;
        public event Action OnDamageTaken = delegate { };

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

        public Transform Transform
        {
            get
            {
                if (tr == null)
                {
                    tr = GetComponent<Transform>();
                }
                return tr;
            }
        }

        public void PerformReset()
        {
            CurrentHP = MaxHP;
        }

        protected virtual void Awake()
        {
            tr = transform;
            EventBus<DamageInfo>.AddActions(transform.root.GetInstanceID(), TakeDamage);
        }

        public static bool TakeDamage(Transform transform, DamageInfo damageInfo)
        {
            return EventBus<DamageInfo>.Raise(transform.GetInstanceID(), damageInfo);
        }

        protected void OnEnable()
        {
            PerformReset();
        }

        protected void OnDisable()
        {
            OnDamageTaken = delegate { };
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (damageInfo.Source.GetTeam(throwOnNullTransform: false) == transform.root.GetTeam())
            {
                return;
            }
            CurrentHP -= damageInfo.Amount;
            if (sendEventOnDamage)
            {
                EventBus<ObjectDamaged>.Raise(Transform.GetInstanceID(), new
                    ObjectDamaged(CurrentHP, CurrentHP - MaxHP));
                OnDamageTaken?.Invoke();
            }
            if (CurrentHP <= 0)
            {
                EventBus<ObjectDestroyed>.Raise(Transform.GetInstanceID(),
                    new(Transform, damageInfo.Source));
            }
        }

        protected void OnDestroy()
        {
            EventBus<DamageInfo>.RemoveBinding(transform.root.GetInstanceID());
        }
    }
}