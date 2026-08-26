using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Level
{
    public class SafeZone : MonoBehaviour
    {
        protected HashSet<Unit> unitsInSafeZone = new();

        protected void OnTriggerEnter(Collider other)
        {
            var unit = other.transform.root.GetComponentViaRegister<Unit>();
            if (unit.Active() && unitsInSafeZone.Add(unit)) unit.SafeZoneCount++;
        }

        protected void OnTriggerExit(Collider other)
        {
            var unit = other.transform.root.GetComponentViaRegister<Unit>();
            if (unit.Active() && unitsInSafeZone.Remove(unit)) unit.SafeZoneCount--;
        }

        protected void OnDisable()
        {
            foreach (var unit in unitsInSafeZone)
            {
                if (!unit.Active()) continue;
                unit.SafeZoneCount--;
            }
            unitsInSafeZone.Clear();
        }
    }
}