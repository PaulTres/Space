using UnityEngine;
using Utilities;
namespace Weapons
{
    [RequireComponent(typeof(LineRenderer))]
    public class Laser : Bullet
    {
        protected float range = 100;
        public Transform Target { get; set; }
        protected LineRenderer lineRenderer;

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
                lineRenderer.SetPosition(1, Target.position);
                Target.TakeDamage(damageInfo);
            }
        }
    }
}