using AudioSystem;
using Effects;
using EventBus;
using Spawning;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Weapons
{
    public class Bullet : MonoBehaviour, IOwnable
    {
        #region Pool
        public enum Type
        {
            Laser,
            BasicShell,
            Missile
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
        public Transform Owner
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
                        gameObject.layer = GlobalConfig.GetBulletLayer(value.gameObject.layer);
                    }
                }
            }
        }
        protected Transform tr;
        public Transform Transform
        {
            get
            {
                if (tr == null) tr = transform;
                return tr;
            }
        }
        protected SoundData soundData;
        protected bool disableOnHit = true;
        protected float lifeTime;
        protected PoolableEffect onHitEffect;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            tr = transform;
            ComponentRegister<IOwnable>.Register(tr, this);
        }

        protected virtual void OnEnable()
        {
            if (soundData != null && soundData.clip != null) SoundManager.TryGetInstance(true).Play(soundData, tr.position);
            else Debug.LogWarning($"{transform} has no sound data!");
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformUpdate);
        }

        public virtual void Initialize(BulletData poolableData)
        {
            disableOnHit = poolableData.DisableOnHit;
            damageInfo.Amount = poolableData.Damage;
            lifeTime = poolableData.LifeTime;
            soundData = poolableData.SoundData;
            poolKey = poolableData.Key;
            onHitEffect = poolableData.OnHitEffect;
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
            collider.transform.root.TakeDamage(damageInfo);
            if (onHitEffect != null)
            {
                if (!PoolableEffect.Get(onHitEffect.PoolKey, out var e)) e = Instantiate(onHitEffect);
                e.transform.position = Transform.position;
                e.transform.localScale = Transform.localScale;
                e.gameObject.SetActive(true);
            }
            if (disableOnHit) gameObject.SetActive(false);
        }
        #endregion
    }
}