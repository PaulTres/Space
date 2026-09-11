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
        public UnityEvent OnAllUnitsSpawned, OnAllUnitsDestroyed;
        protected List<Unit> units;
        protected int unitCount;

        protected void Awake()
        {
            spawners = GetComponentsInChildren<NPCSpawner>(true).ToList();
            SpawnInactive();
            if (spawnOnAwake) Spawn();
        }

        /// <summary>
        /// Fires when a unit in the wave is destroyed.
        /// </summary>
        /// <param name="event"></param>
        public void OnUnitDestroyed(ObjectDestroyed @event)
        {
            //Debug.Log($"Wave.OnUnitDestroyed: {@event}");
            EventBus<ObjectDestroyed>.RemoveActions(@event.VictimID, OnUnitDestroyed);
            unitCount--;
            if (unitCount <= 0)
            {
                //Debug.Log("ConcreteWave destroyed");
                OnAllUnitsDestroyed?.Invoke();
            }
        }

        /// <summary>
        /// Spawns all units without activating them. Called in Awake.
        /// </summary>
        public void SpawnInactive()
        {
            units = new();
            unitCount = 0;
            foreach (var spawner in spawners)
            {
                var unit = spawner.Spawn();
                if (unit == null)
                {
                    Debug.LogError($"{spawner.transform.name} spawned empty unit!");
                    continue;
                }
                //Debug.Log(unit.transform);
                unitCount++;
                units.Add(unit);
                EventBus<ObjectDestroyed>.AddActions(unit.GetTransformInstanceID(), OnUnitDestroyed);
            }
        }

        /// <summary>
        /// Calls ActivateUnits with a delay.
        /// </summary>
        public void Spawn()
        {
            MissionManager.TryGetInstance(true).ExecuteAction(ActivateUnits, delay);
        }

        /// <summary>
        /// Calls SpawnInactive if necessary, then activates all units.
        /// </summary>
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
                }
                while (Physics.CheckSphere(units[i].Position, units[i].SpawnCheckRadius, GlobalConfig.SpawnCheckLayerMask))
                {
                    units[i].Position -= units[i].Forward * units[i].SpawnCheckRadius;
                }
                if (facePlayer && GameManager.Player != null) units[i].Transform.LookAt(GameManager.Player.Position);
                //Debug.Log(units[i].transform);
                units[i].gameObject.SetActive(true);
            }
            units = null;
            OnAllUnitsSpawned?.Invoke();
        }
    }
}