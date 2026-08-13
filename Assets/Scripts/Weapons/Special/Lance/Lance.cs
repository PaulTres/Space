using AudioSystem;
using System;
using Timers;
using UnityEngine;
using Utilities;

namespace Weapons
{
    public class Lance : Bullet, IThreat
    {
        public override ImprovedMonoBehaviour Target => null;
        protected Collider[] buffer;
        protected int colliderCount;
        protected float range, radius;
        protected CountdownTimer telegraphTimer;
        [SerializeField] protected Renderer telegraphRenderer, damageRenderer;
        protected Color telegraphColor, telegraphTransparent, damageColor, damageTransparent;
        protected float maxLifeTime;
        protected SoundData soundData, telegraphSound;

        public event Action<IThreat> OnThreatOver = delegate { };

        protected override void Awake()
        {
            base.Awake();
            buffer = new Collider[GlobalConfig.MaxLanceCheckBufferSize];
        }

        protected override void OnEnable()
        {
            base.OnEnable();
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
            Transform.localScale = new(radius * 2, radius * 2, range / 2);
            disableOnHit = false;
            telegraphColor = data.TelegraphMaterial.color;
            telegraphTransparent = new(telegraphColor.r, telegraphColor.g, telegraphColor.b, 0);
            damageColor = data.DamageMaterial.color;
            damageTransparent = new(damageColor.r, damageColor.g, damageColor.b, 0);
            damageRenderer.material = data.DamageMaterial;
            telegraphRenderer.material = data.TelegraphMaterial;
            damageRenderer.material.color = damageTransparent;
            lifeTime += data.Delay;
            maxLifeTime = lifeTime;
            soundData = data.FireSound;
            telegraphSound = data.TelegraphSound;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            OnThreatOver(this);
            OnThreatOver = delegate { };
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            telegraphTimer?.Dispose();
        }

        protected override void PerformUpdate(float dt)
        {
            if (telegraphTimer.IsRunning)
            {
                //adjust telegraph renderer
                telegraphRenderer.material.color = Color.Lerp(telegraphColor, telegraphTransparent, telegraphTimer.Progress);
                Debug.Log(telegraphRenderer.material.color.a);
            }
            else
            {
                damageRenderer.material.color = Color.Lerp(damageColor, damageTransparent, lifeTime / maxLifeTime);
            }
            base.PerformUpdate(dt);
        }

        public void Telegraph()
        {
            telegraphTimer.Start();
            telegraphSound.Play(Position);
            telegraphRenderer.enabled = true;
            damageRenderer.enabled = false;
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
            OnThreatOver(this);
            telegraphRenderer.enabled = false;
            damageRenderer.enabled = true;
            soundData?.Play(Position);
            telegraphRenderer.material.color = new(0, 0, 0, 0);
            colliderCount = Physics.OverlapCapsuleNonAlloc(Transform.position,
                Position + Forward * range, radius, buffer, GlobalConfig.LanceCheckMask);
            for (int i = 0; i < colliderCount; i++) OnHit(buffer[i]);
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