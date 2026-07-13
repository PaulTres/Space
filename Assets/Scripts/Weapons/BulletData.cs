using Effects;
using Spawning;
using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/BulletData", fileName = "BulletData")]
    public class BulletData : MonoPoolableData<Bullet.Type, Bullet>
    {
        public bool DisableOnHit = true;
        public int Damage;
        public float LifeTime = 20;
        public float EffectScaleFactor = 1;
        public Vector3 EffectScale => EffectScaleFactor * Vector3.one;
        public PoolableEffect OnSpawnEffect;
        public PoolableEffect OnHitEffect;

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
            if (LifeTime <= 0)
            {
                Debug.LogError($"Lifetime <= 0 for {this}!");
                LifeTime = 1;
            }
        }
    }
}