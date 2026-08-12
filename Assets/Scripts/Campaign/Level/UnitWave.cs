using EventBus;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
    [System.Serializable]
    public class UnitWave
    {
        public List<UnitSpawnData> Units = new();
        public event System.Action OnAllUnitsDestroyed = delegate { };
        protected int unitCount;

        public void OnUnitDestroyed(ObjectDestroyed @event)
        {
            //Debug.Log($"Unit destroyed: {@event.VictimName}");
            EventBus<ObjectDestroyed>.RemoveActions(@event.VictimID, OnUnitDestroyed);
            unitCount--;
            if (unitCount <= 0)
            {
                //Debug.Log("Wave destroyed");
                OnAllUnitsDestroyed();
            }
        }

        public void Spawn(int pointLimit = int.MaxValue)
        {
            unitCount = 0;
            foreach (var spawner in Units)
            {
                var choice = spawner.ChooseUnit(pointLimit);
                if (choice == null) continue;
                pointLimit -= choice.Points;
                unitCount++;
                var unit = Mission.CurrentMission.SpawnUnit(choice, spawner.SpawnPosition + UnityEngine.Random.insideUnitSphere *
                      spawner.SpawnRadius, Quaternion.Euler(spawner.SpawnRotation),
                      spawner.Team);
                EventBus<ObjectDestroyed>.AddActions(unit.Transform.root.GetInstanceID(), OnUnitDestroyed);
            }
        }
    }
}