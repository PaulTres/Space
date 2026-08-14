using EventBus;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

namespace Level
{
    public class Wave : MonoBehaviour
    {
        protected List<NPCSpawner> spawners;
        [SerializeField] protected float delay = 0;
        [SerializeField] protected float minDistanceFromPlayer = 0;
        [SerializeField] protected bool facePlayer = false;
        [SerializeField] protected bool spawnOnAwake = true;
        [SerializeField] protected UnityEvent onAllUnitsSpawned, onAllUnitsDestroyed;
        protected List<Unit> units = new();
        protected int unitCount;

        protected void Awake()
        {
            spawners = GetComponentsInChildren<NPCSpawner>().ToList();
            SpawnInactive();
            if (spawnOnAwake) SpawnWithDelay();
        }

        public void OnUnitDestroyed(ObjectDestroyed @event)
        {
            EventBus<ObjectDestroyed>.RemoveActions(@event.VictimID, OnUnitDestroyed);
            unitCount--;
            if (unitCount <= 0)
            {
                //Debug.Log("Wave destroyed");
                onAllUnitsDestroyed?.Invoke();
            }
        }

        public void SpawnInactive()
        {
            unitCount = 0;
            foreach (var spawner in spawners)
            {
                var unit = spawner.Spawn();
                //if (unit == null) continue;
                //Debug.Log(unit.transform);
                unitCount++;
                units.Add(unit);
                EventBus<ObjectDestroyed>.AddActions(unit.Transform.root.GetInstanceID(), OnUnitDestroyed);
            }
        }

        public void SpawnWithDelay()
        {
            MissionManager.TryGetInstance(true).ExecuteAction(ActivateUnits, delay);
        }
        protected void ActivateUnits()
        {
            if (units.IsEmpty()) SpawnInactive();
            for (int i = 0; i < units.Count; i++)
            {
                if (GameManager.Player != null && Vector3.Distance(transform.position,
                    GameManager.Player.Position) < minDistanceFromPlayer)
                {
                    units[i].Position = units[i].Position + (units[i].Position -
                        GameManager.Player.Position).normalized * minDistanceFromPlayer;
                    if (facePlayer) units[i].Transform.LookAt(GameManager.Player.Position);
                }
                //Debug.Log(units[i].transform);
                units[i].gameObject.SetActive(true);
            }
            units = null;
            onAllUnitsSpawned?.Invoke();
        }
    }
}