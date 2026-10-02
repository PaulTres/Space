using Effects;
using Spawning;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Weapons
{
    public interface IOwnable : IRegisterableComponent
    {
        Unit Owner { get; }
    }

    public abstract class Bullet : ImprovedMonoBehaviour, IOwnable
    {
        #region Pool
        public enum Type
        {
            Laser,
            BasicShell,
            RegularMissile,
            ShadowMissile,
            Lance,
            MACRound,
            GuidedShell
        }
        static readonly ObjectPool<Type, Bullet> objectPool = new();

        /// <summary>
        /// Makes the pool clear itself on scene change. Call this method once at the 
        /// start of the game.
        /// </summary>
        public static void InitializePool()
        {
            SceneManager.activeSceneChanged += (s1, s2) => ClearPool();
        }
        public static void ClearPool()
        {
            objectPool.Clear();
        }
        static void Release(Bullet bullet)
        {
            objectPool.Release(bullet.poolKey, bullet);
        }
        public static bool Get(Type bulletType, out Bullet bullet)
        {
            return objectPool.Get(bulletType, out bullet);
        }
        #endregion

        #region Fields
        protected Type poolKey;
        protected DamageInfo damageInfo = new();
        public Unit Owner
        {
            get
            {
                return damageInfo.Source;
            }
            set
            {
                if (value != damageInfo.Source)
                {
                    damageInfo.Source = value;
                    if (value != null)
                    {
                        SetObjectLayer();
                        Team = value.Team;
                    }
                }
            }
        }
        protected bool disableOnHit = true;
        protected float lifeTime;
        protected PoolableEffect onSpawnEffect, onHitEffect;
        protected float effectScaleFactor;
        public override float EffectScaleFactor => effectScaleFactor;
        public override float CurrentSignature => 0;
        public override float ScanRange => 0;
        public override float CurrentHPPercentage => 0;
        #endregion

        #region Setup
        public override void SetObjectLayer()
        {
            gameObject.layer = GlobalConfig.GetBulletLayer(Owner.Team);
        }

        protected virtual void OnEnable()
        {
            onSpawnEffect.SpawnEffect(Transform.position, effectScaleFactor);
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformFixedUpdate);
        }

        public virtual void Initialize(BulletData poolableData)
        {
            poolKey = poolableData.Key;
            disableOnHit = poolableData.DisableOnHit;
            damageInfo.Amount = poolableData.Damage * GlobalConfig.GetDamageModifier(Team);
            lifeTime = poolableData.LifeTime;
            onSpawnEffect = poolableData.OnSpawnEffect;
            onHitEffect = poolableData.OnHitEffect;
            effectScaleFactor = poolableData.EffectScaleFactor;
        }

        protected virtual void OnDisable()
        {
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterFixedUpdate(PerformFixedUpdate);
            Release(this);
        }
        #endregion

        #region Functionality
        protected virtual void PerformFixedUpdate(float dt)
        {
            lifeTime -= dt;
            if (lifeTime <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        protected virtual void OnHit(Collider collider)
        {
            var root = collider.transform.root;
            if (root == Owner.Transform.root) return;
            if (!collider.transform.TakeDamage(damageInfo))
            {
                Debug.LogError($"{collider.transform} on {collider.transform.root} cannot take damage!");
            }
            onHitEffect.SpawnEffect(Transform.position, Transform.rotation, effectScaleFactor);
            if (disableOnHit) gameObject.SetActive(false);
        }
        #endregion
    }
}