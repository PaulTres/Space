using AudioSystem;
using System;
using System.Collections.Generic;
using Timers;
using UnityEngine;
using Utilities;

namespace Weapons
{
    public class LanceBullet : Projectile, IThreat
    {
        protected ImprovedMonoBehaviour target;
        public override ImprovedMonoBehaviour Target => target;
        protected Collider[] buffer;
        protected int colliderCount;
        protected float range, radius;
        protected CountdownTimer telegraphTimer;
        [SerializeField] protected ParticleSystem telegraphVisuals, damageVisuals;
        protected Color telegraphColor, telegraphTransparent, damageColor, damageTransparent;
        protected float maxLifeTime, timerProgressTrackingThreshold, delay;
        protected SoundData soundData, telegraphSound;
        public event Action<IThreat> OnThreatOver = delegate { };

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            buffer = new Collider[GlobalConfig.MaxLanceCheckBufferSize];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            collisionMask = 0;
            Telegraph();
        }

        public override void Initialize(BulletData poolableData)
        {
            base.Initialize(poolableData);
            if (poolableData is not LanceData data)
            {
                Debug.LogError("Invalid poolable data for lance.");
                return;
            }
            if (telegraphTimer == null)
            {
                telegraphTimer = new(data.Delay);
                telegraphTimer.OnTimerStop += DealDamage;
            }
            else telegraphTimer.Reset(data.Delay);
            radius = data.Radius;
            range = data.Range;
            disableOnHit = false;
            delay = data.Delay;
            lifeTime += delay;
            maxLifeTime = lifeTime;
            soundData = data.FireSound;
            telegraphSound = data.TelegraphSound;
            timerProgressTrackingThreshold = data.TimerProgressTrackingThreshold;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            damageVisuals.Stop();
            telegraphVisuals.Stop();
            OnThreatOver(this);
            OnThreatOver = delegate { };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            telegraphTimer?.Dispose();
        }
        #endregion

        public void SetTarget(ImprovedMonoBehaviour target)
        {
            this.target = target;
        }

        protected override void PerformFixedUpdate(float dt)
        {
            if (telegraphTimer.IsRunning)
            {
                if (target.Active() && telegraphTimer.Progress > timerProgressTrackingThreshold)
                {
                    //track target
                    Transform.LookAt(target.Position);
                }
            }
            base.PerformFixedUpdate(dt);
        }

        public void Telegraph()
        {
            telegraphVisuals.Play();
            telegraphTimer.Start();
            telegraphSound.Play(Position);
            colliderCount = Physics.OverlapCapsuleNonAlloc(Transform.position,
                Position + Forward * range, radius, buffer, GlobalConfig.LanceCheckMask);
            for (int i = 0; i < colliderCount; i++)
            {
                var t = buffer[i].transform.root;
                if (t == Owner) continue;
                var bt = buffer[i].transform.root.GetComponentViaRegister<BanditTree>();
                if (bt != null) bt.ReceiveThreat(this);
            }
        }

        public void DealDamage()
        {
            acceleration = 0;
            linearVelocity = Vector3.zero;
            OnThreatOver(this);
            telegraphVisuals.Stop();
            telegraphVisuals.Clear();
            soundData?.Play(Position);
            damageVisuals.Play();
            colliderCount = Physics.OverlapCapsuleNonAlloc(Transform.position,
                Position + Forward * range, radius, buffer, GlobalConfig.LanceCheckMask);
            HashSet<int> hits = new();
            for (int i = 0; i < colliderCount; i++)
            {
                var t = buffer[i].transform.parent != null ? buffer[i].transform.parent : buffer[i].transform;
                if (hits.Add(t.GetInstanceID())) OnHit(buffer[i]);
            }
        }

        public Vector3 DodgeDirection(ImprovedMonoBehaviour threatenedObject)
        {
            var projected = Vector3.ProjectOnPlane(threatenedObject.Forward, Forward);
            if (projected.sqrMagnitude == 0)
            {
                projected = Vector3.ProjectOnPlane(threatenedObject.Right, Forward);
            }
            return projected.normalized;
        }
    }
}