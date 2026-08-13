using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] protected List<UnitData> spawnOptions = new();
        [SerializeField] protected float spawnRadius = 0;
        [SerializeField] protected int team = 2;
        [SerializeField] protected bool playSpawnEffect = true;

        public void OnDrawGizmos()
        {
            Gizmos.color = team == GlobalConfig.PlayerTeam ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(spawnRadius, 1));
        }

        public Unit Spawn()
        {
            return SpawnUnit(spawnOptions[Random.Range(0, spawnOptions.Count)]);
        }

        public Unit Spawn(out int pointCost, int maxPoints = int.MaxValue)
        {
            pointCost = 0;
            var available = spawnOptions.Where(d => d.Points <= maxPoints).ToList();
            if (available.Count == 0) return null;
            var index = Random.Range(0, available.Count);
            pointCost = available[index].Points;
            return SpawnUnit(available[index]);
        }

        protected Unit SpawnUnit(UnitData data)
        {
            if (data == null) return null;
            var unit = data.CreateInstance();
            unit.Team = team;
            unit.PlaySpawnEffect = playSpawnEffect;
            //Debug.Log($"Team of {unit.Transform} is {unit.Team}.");
            unit.Transform.SetPositionAndRotation(transform.position +
                Random.insideUnitSphere * spawnRadius, transform.rotation);
            return unit;
        }
    }
}