using Effects;
using EventBus;
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
            Missile,
            Lance
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
                    if (damageInfo.Source != null) EventBus<PoolableRecycled>.RemoveActions(damageInfo.Source.GetInstanceID(), null, ClearOwner);
                    damageInfo.Source = value;
                    if (value != null)
                    {
                        EventBus<PoolableRecycled>.AddActions(value.GetInstanceID(), null, ClearOwner);
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
            tr = transform;
            ComponentRegister<IOwnable>.Register(tr, this);
        }

        protected virtual void OnEnable()
        {
            onSpawnEffect.SpawnEffect(Transform.position, effectScaleFactor);
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformUpdate);
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

        /// <summary>
        /// Set the owner to null without changing the bullet's layer.
        /// </summary>
        public void ClearOwner()
        {
            Owner = null;
        }

        protected virtual void OnDisable()
        {
            Owner = null;
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterFixedUpdate(PerformUpdate);
            Release(this);
        }

        protected virtual void OnDestroy()
        {
            ComponentRegister<IOwnable>.Unregister(tr);
        }
        #endregion

        #region Functionality
        protected virtual void PerformUpdate(float dt)
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