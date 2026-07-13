using Effects;
using EventBus;
using HP;
using UnityEngine;
using Utilities;
namespace Weapons
{
    [RequireComponent(typeof(HPComponent))]
    [RequireComponent(typeof(SphereCollider))]
    public class Missile : Projectile
    {
        #region Fields
        protected IObject target;
        public IObject Target
        {
            get
            {
                return target;
            }
            set
            {
                if (target != value)
                {
                    if (target != null) MissileManager.Unregister(this);
                    target = value;
                    if (target != null) MissileManager.Register(this);
                }
            }
        }
        protected float initialTracking, maxTargetSignature, maxTelemetryBonus;
        public int Index { get; set; }
        protected HPComponent hpComponent;
        public float EffectiveTracking { get; protected set; }
        protected PoolableEffect effectOnDeath;
        protected TrailRenderer trailRenderer;
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            EventBus<ObjectDestroyed>.AddActions(Extensions.GetInstanceID(this), Die);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ComponentRegister<Missile>.Register(Transform, this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventBus<ObjectDestroyed>.RemoveBinding(Extensions.GetInstanceID(this));
        }

        protected override void OnDisable()
        {
            Target = null;
            ComponentRegister<Missile>.Unregister(Transform);
            base.OnDisable();
        }

        public override void Initialize(BulletData poolableData)
        {
            base.Initialize(poolableData);
            if (poolableData is not MissileData missileData)
            {
                throw new System.ArgumentException($"Expected {typeof(MissileData)}, " +
                    $"got {poolableData.GetType()}");
            }
            initialTracking = missileData.Tracking;
            if (hpComponent == null) hpComponent = GetComponent<HPComponent>();
            hpComponent.MaxHP = missileData.HP;
            maxTargetSignature = missileData.MaxTargetSignature;
            maxTelemetryBonus = missileData.MaxTelemetryBonus;
            effectOnDeath = missileData.OnDeathEffect;
            if (trailRenderer == null) trailRenderer = GetComponent<TrailRenderer>();
            trailRenderer.material = missileData.RendererMaterial;
            trailRenderer.startWidth = Transform.localScale.y;
            trailRenderer.endWidth = 0;
            trailRenderer.time = 1;
        }
        #endregion

        #region Functionality
        protected float Telemetry()
        {
            if (Owner == null) return 0;
            return Mathf.Max(0, Mathf.Min(maxTelemetryBonus, Vector3.Dot(Owner.forward,
                (target.Transform.position - Owner.position).normalized)));
        }

        protected void CalculateEffectiveTracking()
        {
            if (Target == null)
            {
                EffectiveTracking = 0;
                return;
            }

            EffectiveTracking = initialTracking * (1 + Telemetry()) *
                Mathf.Min(1, target.Signature / maxTargetSignature);
        }

        protected override void RestOfPerformUpdate(float dt)
        {
            if (target != null && target.Transform.gameObject.activeInHierarchy == false)
            {
                Target = null;
                EffectiveTracking = 0;
            }
            if (Target != null)
            {
                CalculateEffectiveTracking();

                Vector3 directionToTarget = (Target.Transform.position - tr.position).normalized;
                Vector3 newDirection = Vector3.RotateTowards(tr.forward, directionToTarget,
                    EffectiveTracking * dt, 0);

                Quaternion prevRotation = tr.rotation;
                tr.rotation = Quaternion.LookRotation(newDirection);

                Quaternion rotationDelta = (transform.rotation * Quaternion.Inverse(prevRotation)).normalized;
                rotationDelta.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
                if (angleInDegrees > 180f) angleInDegrees -= 360f;

                angularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / dt;
            }
            base.PerformUpdate(dt);
            linearVelocity += acceleration * dt * tr.forward;
            linearVelocity = Vector3.ClampMagnitude(linearVelocity, topSpeed);
        }

        /// <summary>
        /// Plays an effectOnDeath if the missile has one. Deactivates the game object.
        /// </summary>
        /// <param name="event"></param>
        public void Die(ObjectDestroyed @event)
        {
            if (effectOnDeath != null)
            {
                if (!PoolableEffect.Get(effectOnDeath.PoolKey, out var e)) e = Instantiate(effectOnDeath);
                e.transform.position = Transform.position;
                e.transform.localScale = Transform.localScale;
                e.gameObject.SetActive(true);
            }
            gameObject.SetActive(false);
        }
        #endregion
    }
}