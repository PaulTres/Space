using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Level
{
    public class SafeZone : MonoBehaviour
    {
        protected static HashSet<SafeZone> safeZones = new();

        public static bool InSafeZone(ImprovedMonoBehaviour unit)
        {
            if (!unit.Active()) return false;
            return InSafeZone(unit.Position);
        }

        public static bool InSafeZone(Vector3 position)
        {
            foreach (var zone in safeZones)
            {
                if (zone == null || !zone.enabled) continue;
                if (Vector3.Distance(position, zone.Position) <= zone.radius) return true;
            }
            return false;
        }

        [SerializeField] protected float radius = 1000;
        protected Transform tr;
        public Transform Transform
        {
            get
            {
                if (tr == null) tr = transform;
                return tr;
            }
        }
        public Vector3 Position => Transform.position;

        protected void OnEnable() => safeZones.Add(this);

        protected void OnDisable() => safeZones.Remove(this);

        protected void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(Position, radius);
        }
    }
}