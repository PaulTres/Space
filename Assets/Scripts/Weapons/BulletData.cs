using Effects;
using Spawning;
using UnityEngine;
namespace Weapons
{
    public abstract class BulletData : MonoPoolableData<Bullet.Type, Bullet>
    {
        public bool ValidateData = true;
        [Tooltip("Transform will be disabled on hit if true.")]
        public bool DisableOnHit = true;
        public float Damage = 1;
        public float LifeTime = 10;
        [Tooltip("Scale of effects spawned by this bullet. Affects the scale of the bullet.")]
        public float EffectScaleFactor = 1;
        public Vector3 EffectScale => EffectScaleFactor * Vector3.one;
        public PoolableEffect OnSpawnEffect;
        public PoolableEffect OnHitEffect;
        public bool DamagePriorityTargets = true;
        /// <summary>
        /// Roughly how fast the bullet can hit the target. Used for AI decision making.
        /// </summary>
        public abstract float Speed { get; }

        /// <summary>
        /// Returns an instance of the bullet from the object pool or creates a new one if 
        /// it doesn't exist. Initializes the bullet with the current BulletData.
        /// </summary>
        /// <returns>The initialized bullet instance.</returns>
        protected override Bullet GetInstance()
        {
            if (!Bullet.Get(Key, out Bullet instance))
            {
                instance = CreateInstance();
            }
            instance.Initialize(this);
            return instance;
        }

        protected virtual void OnValidate()
        {
            if (!ValidateData) return;
            if (LifeTime <= 0)
            {
                Debug.LogError($"Lifetime <= 0 for {this}!");
                LifeTime = 1;
            }
            if (Prefab == null) Debug.LogWarning($"{this} has no prefab!");
            else if (Prefab.gameObject.activeSelf)
            {
                Debug.LogWarning($"{this} has active prefab!");
            }
            if (OnSpawnEffect == null) Debug.LogWarning($"{this} has no effect on spawn!");
        }
    }
}