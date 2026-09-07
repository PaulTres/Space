using AudioSystem;
using Spawning;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
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
            ExplosionBigPurple,
            ExplosionBigGreen,
            ExplosionBigRed,
            JumpEffect,
            ShadowPDLaserShot,
            ShadowExplosionSmall,
            HumanExplosionSmall,
            MissileLaunchYellow
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

        public static void Spawn(PoolableEffect effect, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (effect == null) return;
            if (!Get(effect.PoolKey, out var e)) e = Instantiate(effect);
            e.Transform.SetPositionAndRotation(position, rotation);
            e.Transform.localScale = scale;
            e.gameObject.SetActive(true);
        }
        #endregion

        [field: SerializeField] public Type PoolKey { get; protected set; }
        protected ParticleSystem[] particles;
        public ParticleSystem[] Particles
        {
            get
            {
                if (particles.IsEmpty() && !audioOnly)
                {
                    particles = GetComponentsInChildren<ParticleSystem>();
                    foreach (var particle in particles)
                    {
                        var p = particle.main;
                        p.loop = false;
                        p.stopAction = ParticleSystemStopAction.Disable;
                    }
                }
                return particles;
            }
        }
        [SerializeField] protected SoundData soundData;
        [Tooltip("If set to true, the script will not check for particle effects with GetComponent and will only play the sound effect.")]
        [SerializeField] protected bool audioOnly = false;
        protected Transform tr;
        public Transform Transform
        {
            get
            {
                if (tr == null) tr = transform;
                return tr;
            }
        }

        protected void OnEnable()
        {
            //Debug.Log($"Playing effect: {PoolKey}");
            soundData.Play(transform.position);
            if (!Particles.IsEmpty())
            {
                for (int i = 0; i < Particles.Length; i++)
                {
                    Particles[i].Play();
                }
            }
        }
        protected void OnDisable()
        {
            Release(this);
        }
    }
}