using AudioSystem;
using Spawning;
using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/BulletData", fileName = "BulletData")]
    public class BulletData : MonoPoolableData<Bullet.Type, Bullet>
    {
        public bool DisableOnHit = true;
        public int Damage;
        public float LifeTime;
        public SoundData SoundData;

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