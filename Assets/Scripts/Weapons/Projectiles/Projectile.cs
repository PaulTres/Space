using UnityEngine;
namespace Weapons
{
    public class Projectile : Bullet, IObject
    {
        #region Fields
        public IObject.Type ObjectType => IObject.Type.Missile;
        protected float topSpeed, acceleration;
        public virtual float TopSpeed => topSpeed;
        protected Vector3 linearVelocity, angularVelocity;
        public Vector3 Forward => Transform.forward;
        public Vector3 Right => Transform.right;
        public Vector3 Up => Transform.up;
        public Vector3 LinearVelocity => linearVelocity;
        public Vector3 AngularVelocity => Vector3.zero;
        public float Signature => 0;
        protected LayerMask collisionMask = 1 << 0 | 1 << 6;
        protected float initialVelocity;
        protected TrailRenderer trailRenderer;
        protected bool inertialDampeners = true;
        #endregion

        #region Setup
        public override void Initialize(BulletData poolableData)
        {
            base.Initialize(poolableData);
            if (poolableData is not ProjectileData data)
            {
                throw new System.ArgumentException($"Expected {typeof(ProjectileData)}, got {poolableData.GetType()}");
            }
            topSpeed = data.TopSpeed;
            initialVelocity = data.InitialVelocity;
            if (topSpeed <= 0)
            {
                Debug.LogWarning($"Missile top speed set to 0 or lower on {transform}");
                if (topSpeed < 0) topSpeed = 0;
            }
            acceleration = data.Acceleration;
            if (acceleration <= 0)
            {
                Debug.LogWarning($"Projectile acceleration should not be 0. Setting acceleration to 10% of top speed on {transform}.");
                acceleration = topSpeed / 10;
            }
            Transform.localScale = data.Scale;

            if (trailRenderer == null) trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer != null)
            {
                trailRenderer.material = data.RendererMaterial;
                trailRenderer.startWidth = Transform.localScale.y;
                trailRenderer.endWidth = 0;
                trailRenderer.time = 1;
            }
            inertialDampeners = data.InertialDampeners;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            linearVelocity = Forward * initialVelocity;
            if (Owner != null) gameObject.layer = GlobalConfig.
                    GetBulletLayer(Owner.gameObject.layer);
            else gameObject.layer = GlobalConfig.GetBulletLayer(0);
            collisionMask = GlobalConfig.GetBulletCollisionMask(gameObject.layer);
            angularVelocity = Vector3.zero;
        }
        #endregion

        #region Functionality
        protected override void PerformUpdate(float dt)
        {
            base.PerformUpdate(dt);
            if (!CheckCollisions(dt)) RestOfPerformUpdate(dt);
        }

        /// <summary>
        /// Updates and applies linear velocity.
        /// </summary>
        /// <param name="dt"></param>
        protected virtual void RestOfPerformUpdate(float dt)
        {
            linearVelocity += acceleration * dt * tr.forward;
            if (inertialDampeners)
            {
                linearVelocity = Vector3.Project(linearVelocity, tr.forward);
            }
            linearVelocity = Vector3.ClampMagnitude(linearVelocity, TopSpeed);
            tr.position += dt * linearVelocity;
        }

        /// <summary>
        /// Checks for collisions using a raycast in the direction of travel.
        /// If found, returns disableOnHit and calls OnHit.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        protected virtual bool CheckCollisions(float dt)
        {
            if (Physics.Raycast(tr.position, linearVelocity, out RaycastHit hit,
                linearVelocity.magnitude * dt, collisionMask))
            {
                if (GlobalConfig.GetTeam(hit.transform.root.gameObject.layer) == Owner.Team)
                {
                    return false;
                }
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