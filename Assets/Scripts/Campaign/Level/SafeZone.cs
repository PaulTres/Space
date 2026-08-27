using EventBus;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Level
{
    public class SafeZone : MonoBehaviour
    {
        protected static HashSet<SafeZone> safeZones = new();

        public static bool InSafeZone(Unit unit)
        {
            if (!unit.Active()) return false;
            foreach (var zone in safeZones)
            {
                if (zone == null || !zone.enabled) continue;
                if (Vector3.Distance(unit.Position, zone.Position) <= zone.radius) return true;
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

        protected void OnEnable()
        {
            EventBus<ObjectDestroyed>.AddActions(Transform.root.GetInstanceID(), null, OnDeath);
            safeZones.Add(this);
        }

        protected void OnDisable()
        {
            safeZones.Remove(this);
            EventBus<ObjectDestroyed>.RemoveActions(Transform.root.GetInstanceID(), null, OnDeath);
        }

        protected void OnDeath()
        {
            gameObject.SetActive(false);
        }

        protected void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Position, radius);
        }
    }
}