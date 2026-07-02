using AudioSystem;
using Spawning;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Effects
{
    public class PoolableEffect : MonoBehaviour
    {
        #region Pool
        public enum Type
        {
            MissileExplosion
        }
        static readonly ObjectPool<Type, PoolableEffect> objectPool = new();

        public static void InitializePool()
        {
            SceneManager.activeSceneChanged += (s1, s2) => ClearPool();
        }

        public static void ClearPool()
        {
            objectPool.Clear();
        }

        static void Release(PoolableEffect effect)
        {
            objectPool.Release(effect.PoolKey, effect);
        }

        public static bool Get(Type effectType, out PoolableEffect effect)
        {
            return objectPool.Get(effectType, out effect);
        }
        #endregion

        [field: SerializeField] public Type PoolKey { get; protected set; }
        protected WaitForSeconds wait;
        protected Coroutine coroutine;
        protected ParticleSystem[] particles;
        [SerializeField] protected SoundData soundData;

        protected void Awake()
        {
            wait = new(GlobalConfig.ParticleSystemLifetimeCheckInterval);
            particles = new ParticleSystem[particles.Length];
        }

        protected void OnEnable()
        {
            if (soundData != null) SoundManager.TryGetInstance(true).Play(soundData, transform.position);
            if (particles != null)
            {
                for (int i = 0; i < particles.Length; i++)
                {
                    particles[i].Play();
                }
            }
            coroutine = StartCoroutine(LifetimeCheck());
        }

        protected IEnumerator LifetimeCheck()
        {
            if (particles == null)
            {
                gameObject.SetActive(false);
                yield break;
            }
            bool stop;
            while (true)
            {
                yield return wait;
                stop = true;
                for (int i = 0; i < particles.Length && stop; i++)
                {
                    if (particles[i].isPlaying) stop = false;
                }
                if (stop)
                {
                    gameObject.SetActive(false);
                    yield break;
                }
            }
        }

        protected void OnDisable()
        {
            if (coroutine != null) StopCoroutine(coroutine);
            Release(this);
        }
    }
}