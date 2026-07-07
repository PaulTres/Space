using UnityEngine;
using Utilities;
namespace Weapons
{
    public class Projectile : Bullet, IObject
    {
        public IObject.Type ObjectType => IObject.Type.Missile;
        protected float topSpeed, acceleration;
        protected Vector3 linearVelocity, angularVelocity;
        public Vector3 Forward => Transform.forward;
        public Vector3 Right => Transform.right;
        public Vector3 Up => Transform.up;
        public Vector3 LinearVelocity => linearVelocity;
        public Vector3 AngularVelocity => Vector3.zero;
        public float Signature => 0;
        protected LayerMask collisionMask = 1 << 0 | 1 << 6;
        public float initialVelocity;

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

            this.RegisterForRadar();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.UnregisterForRadar();
        }

        protected override void PerformUpdate(float dt)
        {
            base.PerformUpdate(dt);
            if (!CheckCollisions(dt)) RestOfPerformUpdate(dt);
        }

        protected virtual void RestOfPerformUpdate(float dt)
        {
            linearVelocity += acceleration * dt * tr.forward;
            linearVelocity = Vector3.ClampMagnitude(linearVelocity, topSpeed);
            tr.position += dt * linearVelocity;
        }

        protected virtual bool CheckCollisions(float dt)
        {
            if (Physics.Raycast(tr.position, linearVelocity, out RaycastHit hit,
                linearVelocity.magnitude * dt, collisionMask))
            {
                if (GlobalConfig.GetTeam(hit.transform.root.gameObject.layer) == Owner.GetTeam())
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
            Gizmos.DrawRay(tr.position, linearVelocity);
            //Gizmos.color = Color.blue;
            //Gizmos.DrawRay(tr.position, tr.forward * topSpeed);
        }
    }
}