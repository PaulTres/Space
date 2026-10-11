using AudioSystem;
using Spawning;
using Timers;
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
            MissileLaunchYellow,
            HumanCannonShot,
            HumanLaserShot,
            MissileLaunchShadow,
            ShadowCannonShot,
            HumanExplosionLarge,
            ShadowExplosionLarge
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
            //Debug.Log($"{effect}:{playSound}");
            e.Transform.localScale = scale;
            e.Transform.SetPositionAndRotation(position, rotation);
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
                        p.scalingMode = ParticleSystemScalingMode.Hierarchy;
                        p.useUnscaledTime = true;
                    }
                }
                return particles;
            }
        }
        [SerializeField] protected SoundData soundData;
        [Tooltip("If set to true, the script will not check for particle effects with GetComponent and will only play the sound effect.")]
        [SerializeField] protected bool audioOnly = false;
        [SerializeField] protected float lifetime = 5;
        [SerializeField] protected bool playAtPlayerLocation = false;
        protected Transform tr;
        public Transform Transform
        {
            get
            {
                if (tr == null) tr = transform;
                return tr;
            }
        }
        protected CountdownTimer lifetimeTimer;

        protected void OnEnable()
        {
            if (lifetimeTimer == null)
            {
                lifetimeTimer = new(lifetime);
                lifetimeTimer.OnTimerStop += () => gameObject.SetActive(false);
            }
            lifetimeTimer.Start();

            if (playAtPlayerLocation && GameManager.Player.Active())
            {
                soundData.Play(GameManager.Player.Position);
            }
            else soundData.Play(transform.position);
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
            lifetimeTimer?.Pause();
            Release(this);
        }

        protected void OnDestroy()
        {
            lifetimeTimer?.Dispose();
        }
    }
}