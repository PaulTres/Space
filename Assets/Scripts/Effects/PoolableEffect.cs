using AudioSystem;
using Spawning;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Effects
{
    public class PoolableEffect : MonoBehaviour
    {
        #region Pool
        public enum Type
        {
            MissileExplosionYellow,
            MissileExplosionPurple,
            MissileExplosionBlue,
            GunshotBlue,
            ExplosionBigGreen,
            ExplosionBigRed
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

        public static void Spawn(PoolableEffect effect, Transform parent, Vector3 scale)
        {
            if (effect != null)
            {
                if (!Get(effect.PoolKey, out var e)) e = Instantiate(effect);
                e.transform.parent = parent;
                e.transform.position = parent.position;
                e.transform.localScale = scale;
                e.gameObject.SetActive(true);
            }
        }

        public static void Spawn(PoolableEffect effect, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (effect != null)
            {
                if (!Get(effect.PoolKey, out var e)) e = Instantiate(effect);
                e.transform.SetPositionAndRotation(position, rotation);
                e.transform.localScale = scale;
                e.gameObject.SetActive(true);
            }
        }
        #endregion

        [field: SerializeField] public Type PoolKey { get; protected set; }
        protected ParticleSystem[] particles;
        [SerializeField] protected SoundData soundData;

        protected void Awake()
        {
            particles = GetComponentsInChildren<ParticleSystem>();
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
        }
        protected void OnDisable()
        {
            Release(this);
        }
    }
}