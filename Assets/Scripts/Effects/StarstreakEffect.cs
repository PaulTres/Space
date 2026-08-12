using EventBus;
using Player;
using UnityEngine;
using Utilities;
namespace Effects
{
    [RequireComponent(typeof(ParticleSystem))]
    public class StarstreakEffect : MonoBehaviour
    {
        protected ParticleSystem particles;
        protected PlayerShip player;
        [SerializeField] protected float maxParticles = 10;
        ParticleSystem.MainModule main;

        protected void Awake()
        {
            player = transform.root.GetComponent<PlayerShip>();
            Debug.Assert(player != null);
            EventBus<ObjectDestroyed>.AddActions(transform.root.GetInstanceID(), null, Stop);
            particles = GetComponent<ParticleSystem>();
            main = particles.main;
            GlobalUpdater.TryGetInstance(true).RegisterUpdate(PerformUpdate);
            transform.localScale = Vector3.one * player.EffectScaleFactor;
        }

        protected void PerformUpdate(float deltaTime)
        {
            var ratio = player.LinearVelocity.magnitude / player.MaxVelocityMagnitude;
            main.maxParticles = (int)Mathf.Lerp(0, maxParticles, ratio);
            var e = particles.emission;
            e.rateOverTime = (int)Mathf.Lerp(0, maxParticles, ratio);
            main.simulationSpeed = ratio;
            transform.LookAt(transform.position + player.LinearVelocity);
        }

        protected void Stop()
        {
            particles.Stop();
            EventBus<ObjectDestroyed>.RemoveActions(transform.root.GetInstanceID(), null, Stop);
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterUpdate(PerformUpdate);
        }
    }
}