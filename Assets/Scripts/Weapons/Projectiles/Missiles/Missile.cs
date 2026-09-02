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
        protected ImprovedMonoBehaviour target;
        public override ImprovedMonoBehaviour Target => target;
        protected float initialTracking, maxTargetSignature, maxTelemetryBonus;
        public int Index { get; set; }
        protected HPComponent hpComponent;
        public HPComponent HPComponent
        {
            get
            {
                if (hpComponent == null) hpComponent = GetComponent<HPComponent>();
                return hpComponent;
            }
        }
        public float EffectiveTracking { get; protected set; }
        protected PoolableEffect onDeathEffect;
        public override float TopSpeed => topSpeed * (1 + telemetryBonus);
        protected float telemetryBonus = 0;
        protected bool showOnRadar = false;
        protected float targetedDamage;
        /// <summary>
        /// How much damage is this targeted by. Used to prevent all turrets from 
        /// shooting at the same missile. Replaced old system to account for tanky 
        /// missiles.
        /// </summary>
        public float TargetedDamage
        {
            get => targetedDamage;
            set => targetedDamage = Mathf.Max(value, 0);
        }
        public float MaxHP => HPComponent.MaxHP;
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
            telemetryBonus = 0;
            if (showOnRadar) RegisterForRadar();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ComponentRegister<Missile>.Unregister(Transform);
            EventBus<ObjectDestroyed>.RemoveBinding(Extensions.GetInstanceID(this));
        }

        protected override void OnDisable()
        {
            SetTarget(null);
            TargetedDamage = 0;
            if (showOnRadar) UnregisterForRadar();
            base.OnDisable();
        }

        public void SetTarget(ImprovedMonoBehaviour newTarget)
        {
            target = newTarget;
        }

        public override void Initialize(BulletData poolableData)
        {
            TargetedDamage = 0;
            ComponentRegister<Missile>.Register(Transform, this);
            base.Initialize(poolableData);
            if (poolableData is not MissileData missileData)
            {
                throw new System.ArgumentException($"Expected {typeof(MissileData)}, " +
                    $"got {poolableData.GetType()}");
            }
            initialTracking = missileData.Tracking;
            HPComponent.MaxHP = missileData.HP;
            maxTargetSignature = missileData.MaxTargetSignature;
            maxTelemetryBonus = missileData.MaxTelemetryBonus;
            onDeathEffect = missileData.OnDeathEffect;
            showOnRadar = missileData.ShowOnRadar;
        }
        #endregion

        #region Functionality
        protected float Telemetry()
        {
            if (Owner != null || !Owner.InOptimalFiringRange(target)) return 0;
            return Mathf.Max(0, Mathf.Min(maxTelemetryBonus, Vector3.Dot(
                Owner.TelemetryOrigin.forward, (target.Transform.position -
                Owner.TelemetryOrigin.position).normalized)));
        }

        protected void CalculateEffectiveTracking()
        {
            if (Target == null)
            {
                EffectiveTracking = 0;
                return;
            }

            EffectiveTracking = initialTracking * (1 + telemetryBonus) * (1 +
                Mathf.Min(1, target.CurrentSignature / (maxTargetSignature + Mathf.Epsilon)));
        }

        /// <summary>
        /// Calculates and applies tracking and linear velocity.
        /// </summary>
        /// <param name="dt"></param>
        protected override void RestOfPerformFixedUpdate(float dt)
        {
            if (Target.Active())
            {
                telemetryBonus = Telemetry();
                CalculateEffectiveTracking();

                Vector3 directionToTarget = (Target.Transform.position - Transform.position).normalized;

                //Quaternion newRotation = Quaternion.RotateTowards(tr.rotation, Quaternion.l)
                Vector3 newDirection = Vector3.RotateTowards(Transform.forward, directionToTarget,
                    EffectiveTracking * dt, 0);

                Quaternion prevRotation = Transform.rotation;
                Transform.rotation = Quaternion.LookRotation(newDirection);

                Quaternion rotationDelta = (transform.rotation * Quaternion.Inverse(prevRotation)).normalized;
                rotationDelta.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
                if (angleInDegrees > 180f) angleInDegrees -= 360f;

                angularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / dt;
            }
            base.RestOfPerformFixedUpdate(dt);
        }

        /// <summary>
        /// Plays an onDeathEffect if the missile has one. Deactivates the game object.
        /// </summary>
        /// <param name="event"></param>
        public void Die(ObjectDestroyed @event)
        {
            onDeathEffect.SpawnEffect(Transform.position, effectScaleFactor);
            gameObject.SetActive(false);
        }
        #endregion
    }
}