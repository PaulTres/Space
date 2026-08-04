using UnityEngine;
using Utilities;
namespace Weapons
{
    [RequireComponent(typeof(LineRenderer))]
    public class Laser : Bullet
    {
        protected float range = 100;

        protected LineRenderer lineRenderer;

        protected ImprovedMonoBehaviour target;
        public override ImprovedMonoBehaviour Target => target;

        public void SetTarget(ImprovedMonoBehaviour target)
        {
            this.target = target;
        }

        public override void Initialize(BulletData poolableData)
        {
            base.Initialize(poolableData);
            if (poolableData is not LaserData laserData)
            {
                Debug.LogError("Invalid poolable data for Laser.");
                return;
            }
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.startColor = laserData.Color;
            lineRenderer.endColor = laserData.Color;
            lineRenderer.startWidth = laserData.LineWidth;
            lineRenderer.endWidth = laserData.LineWidth;
            range = laserData.Range;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            lineRenderer.SetPosition(0, Transform.position);
            if (Target == null) lineRenderer.SetPosition(1, Transform.position + Transform.forward * range);
            else
            {
                damageInfo.Position = Transform.position;
                lineRenderer.SetPosition(1, Target.Position);
                Target.Transform.TakeDamage(damageInfo);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SetTarget(null);
        }
    }
}