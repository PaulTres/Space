using UnityEngine;
using Utilities;
namespace Weapons
{
    public class Projectile : Bullet
    {
        #region Fields
        public override ImprovedMonoBehaviour Target => null;
        protected float topSpeed, acceleration;
        public virtual float TopSpeed => topSpeed;
        public virtual float Acceleration => acceleration;
        protected Vector3 linearVelocity, angularVelocity;
        protected LayerMask collisionMask = 1 << 0 | 1 << 6;
        protected float initialVelocity;
        protected TrailRenderer trailRenderer;
        public TrailRenderer TrailRendererProperty
        {
            get
            {
                if (trailRenderer == null) trailRenderer = GetComponent<TrailRenderer>();
                return trailRenderer;
            }
        }
        protected Renderer projectileRenderer;
        public Renderer ProjectileRendererProperty
        {
            get
            {
                if (projectileRenderer == null) projectileRenderer = GetComponent<Renderer>();
                return projectileRenderer;
            }
        }
        protected bool inertialDampeners = true;
        protected int team;
        #endregion

        #region Setup
        public override void Initialize(BulletData poolableData)
        {
            base.Initialize(poolableData);
            if (poolableData is not ProjectileData data)
            {
                throw new System.ArgumentException($"Expected {typeof(ProjectileData)}, got {poolableData.GetType()}");
            }
            var speedModifier = GlobalConfig.GetProjectileSpeedModifier(Team);
            topSpeed = data.TopSpeed * speedModifier;
            if (topSpeed <= 0 && data.ValidateData)
            {
                Debug.LogWarning($"Projectile top speed set to 0 or lower on {transform}");
                if (topSpeed < 0) topSpeed = 0;
            }
            initialVelocity = data.InitialVelocity * speedModifier;
            acceleration = data.Acceleration * speedModifier;
            if (acceleration <= 0 && data.ValidateData)
            {
                Debug.LogWarning($"Projectile acceleration should not be 0. Setting acceleration to 10% of top speed on {transform}.");
                acceleration = topSpeed / 10;
            }
            Transform.localScale = Vector3.one * data.ScaleFactor;
            ProjectileRendererProperty.material = data.RendererMaterial;
            TrailRendererProperty.material = data.TrailRendererMaterial;
            TrailRendererProperty.widthCurve = data.TrailRendererCurve;
            inertialDampeners = data.InertialDampeners;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            linearVelocity = Forward * initialVelocity;
            if (Owner.Active())
            {
                team = Owner.Team;
                gameObject.layer = GlobalConfig.GetBulletLayer(Owner.Team);
            }
            else
            {
                team = 0;
                gameObject.layer = GlobalConfig.GetBulletLayer(0);
            }
            collisionMask = GlobalConfig.GetBulletCollisionMask(gameObject.layer);
            angularVelocity = Vector3.zero;
            TrailRendererProperty.Clear();
        }
        #endregion

        #region Functionality
        protected override void PerformFixedUpdate(float dt)
        {
            base.PerformFixedUpdate(dt);
            if (!CheckCollisions(dt)) RestOfPerformFixedUpdate(dt);
        }

        /// <summary>
        /// Updates and applies linear velocity.
        /// </summary>
        /// <param name="dt"></param>
        protected virtual void RestOfPerformFixedUpdate(float dt)
        {
            linearVelocity += Acceleration * dt * Transform.forward;
            if (inertialDampeners)
            {
                linearVelocity = Vector3.Project(linearVelocity, Transform.forward);
            }
            linearVelocity = Vector3.ClampMagnitude(linearVelocity, TopSpeed);
            Transform.position += dt * linearVelocity;
        }

        /// <summary>
        /// Checks for collisions using a raycast in the direction of travel.
        /// If found, returns disableOnHit and calls OnHit.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        protected virtual bool CheckCollisions(float dt)
        {
            if (Physics.Raycast(Transform.position, linearVelocity, out RaycastHit hit,
                linearVelocity.magnitude * dt, collisionMask))
            {
                if (GlobalConfig.GetTeam(hit.transform.root.gameObject.layer) == team)
                {
                    return false;
                }
                damageInfo.Position = hit.point;
                OnHit(hit.collider);
                return disableOnHit;
            }
            return false;
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Transform.position, linearVelocity);
            //Gizmos.color = TextColor.blue;
            //Gizmos.DrawRay(tr.position, tr.forward * topSpeed);
        }
        #endregion
    }
}