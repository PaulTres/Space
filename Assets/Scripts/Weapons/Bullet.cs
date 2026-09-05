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
            MACRound
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
                if (damageInfo.Source == null)
                {
                    return null;
                }
                return damageInfo.Source;
            }
            set
            {
                if (value != damageInfo.Source)
                {
                    damageInfo.Source = value;
                    if (value != null)
                    {
                        gameObject.layer = GlobalConfig.GetBulletLayer(value.Team);
                    }
                }
            }
        }

        protected bool disableOnHit = true;
        protected float lifeTime;
        protected PoolableEffect onSpawnEffect, onHitEffect;
        protected float effectScaleFactor;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            ComponentRegister<IOwnable>.Register(Transform, this);
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
            damageInfo.Amount = poolableData.Damage;
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

        protected virtual void OnDestroy()
        {
            ComponentRegister<IOwnable>.Unregister(Transform);
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
            var t = collider.transform.root;
            if (t == Owner) return;
            collider.transform.root.TakeDamage(damageInfo);
            onHitEffect.SpawnEffect(Transform.position, Transform.rotation, effectScaleFactor);
            if (disableOnHit) gameObject.SetActive(false);
        }
        #endregion
    }
}