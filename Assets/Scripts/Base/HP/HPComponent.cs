using AudioSystem;
using EventBus;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace HP
{
    public class HPComponent : MonoBehaviour, IResettable
    {
        [Serializable]
        public class HPBlock
        {
            [field: SerializeField] public float MaxHP { get; protected set; }
            [SerializeField] protected float currentHP;
            public float CurrentHP
            {
                get => currentHP;
                set
                {
                    value = Mathf.Clamp(value, 0f, MaxHP);
                    if (currentHP != value)
                    {
                        currentHP = value;
                        if (currentHP <= 0 && root.IsPlayer()) GlobalConfig.AudioOnHPBlockPop.Play(root.position, requiresPlayer: true);
                    }
                }
            }
            public float CurrentHPPercentage => CurrentHP / MaxHP;
            protected Transform root;

            public HPBlock(float maxHP, Transform root)
            {
                MaxHP = maxHP;
                this.root = root;
            }
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
        [SerializeField] protected int hpBlockCount = 1;
        [Tooltip("The time it takes for min hp regain to kick in.")]
        [SerializeField] protected float minHPRegainCooldown = 5;
        [SerializeField] protected bool regenerating = false;
        [SerializeField] protected float regenRate = 1;
        protected float currentHP;
        public float CurrentHP
        {
            get
            {
                return currentHP;
            }
            set
            {
                currentHP = 0;
                for (int i = 0; i < hpBlockCount; i++)
                {
                    var val = Mathf.Clamp(value, 0, HPBlocks[i].MaxHP);
                    HPBlocks[i].CurrentHP = val;
                    value -= val;
                    currentHP += HPBlocks[i].CurrentHP;
                    if (value <= 0) break;
                }
                UpdateDamageEffects();
            }
        }
        public float CurrentHPPercentage => CurrentHP / MaxHP;
        protected float lastDamageTime = -1;
        [field: SerializeField] public List<HPBlock> HPBlocks { get; protected set; } = new();
        [SerializeField] protected SoundData audioOnHPBlockPop;
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
        [SerializeField] protected float effectScaleFactor = .5f;
        [Tooltip("If true, this entity will not take damage.")]
        [SerializeField] protected bool invulnerable = false;
        [SerializeField] protected bool debug = false;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            if (minHPRegainCooldown >= 0)
            {
                GlobalUpdater.TryGetInstance(true).RegisterUpdate(PerformRegeneration);
            }
            if (damageEffects.IsEmpty()) damageEffects = new(Transform.root.GetComponentsInChildren<DamageEffect>(true));
        }

        public void PerformReset()
        {
            lastDamageTime = -1;
            firedEvent = false;
            MaxHP *= GlobalConfig.GetHPModifier(ImprovedMonoBehaviour.Team);
            if (HPBlocks.Count != hpBlockCount)
            {
                HPBlocks.Clear();
                for (int i = 0; i < hpBlockCount; i++)
                {
                    var block = new HPBlock(MaxHP / hpBlockCount, transform.root);
                    HPBlocks.Add(block);
                }
            }
            CurrentHP = MaxHP * startingHPFraction;
        }

        protected void OnEnable() => PerformReset();

        protected void OnDestroy()
        {
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterUpdate(PerformRegeneration);
        }
        #endregion

        public void PerformRegeneration(float deltaTime)
        {
            if (!regenerating) return;
            if (Time.time - lastDamageTime < minHPRegainCooldown) return;
            IncreaseCurrentHP(deltaTime * regenRate);
        }

        public static bool TakeDamage(Transform transform, DamageInfo damageInfo)
        {
            return EventBus<DamageInfo>.Raise(transform.GetInstanceID(), damageInfo);
        }

        protected void IncreaseCurrentHP(float value)
        {
            for (int i = HPBlocks.Count - 1; i >= 0; i--)
            {
                if (HPBlocks[i].CurrentHP > 0 && HPBlocks[i].CurrentHP < HPBlocks[i].MaxHP)
                {
                    currentHP += Mathf.Min(value, HPBlocks[i].MaxHP - HPBlocks[i].CurrentHP);
                    HPBlocks[i].CurrentHP += value;
                    break;
                }
            }
            UpdateDamageEffects();
        }

        protected void ReduceCurrentHP(float value)
        {
            for (int i = HPBlocks.Count - 1; i >= 0; i--)
            {
                if (HPBlocks[i].CurrentHP > 0)
                {
                    currentHP -= Mathf.Min(value, HPBlocks[i].CurrentHP);
                    HPBlocks[i].CurrentHP -= value;
                    break;
                }
            }
            UpdateDamageEffects();
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (invulnerable) return;
            if (damageInfo.Source != null && damageInfo.Source.Team == transform.root.GetTeam())
            {
                if (debug) Debug.Log("Friendly fire");
                return;
            }
            if (ImprovedMonoBehaviour.IsDying) return;
            if (debug) Debug.Log($"{this} took {damageInfo.Amount} damage from {damageInfo.Source}.");

            ReduceCurrentHP(damageInfo.Amount);

            lastDamageTime = Time.time;
            if (!firedEvent && HPBlocks[0].CurrentHP <= 0)
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
                damageEffect.gameObject.SetActive(CurrentHP / MaxHP <
                    damageEffect.DamageThreshold);
            }
        }

    }
}