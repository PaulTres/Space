using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Level
{
    public class NPCSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnerInput
        {
            [Tooltip("Used to make things easier to track. Do not change in inspector.")]
            public string Difficulty;
            public List<UnitData> Options = new();

            public SpawnerInput(List<UnitData> options) => Options = options;
        }

        [Tooltip("The system will pick from the option closest to the current difficulty: 0-Easy; 1-Normal;2-Hard.")]
        [SerializeField] protected List<SpawnerInput> spawnOptions = new();
        [SerializeField] protected float spawnRadius = 0;
        [SerializeField] protected int team = 2;
        [field: SerializeField] public bool PlaySpawnEffect { get; set; } = true;
        [SerializeField] protected bool criticalObjective = false;

        protected void OnValidate()
        {
            for (int i = 0; i < spawnOptions.Count; i++)
            {
                List<PlayerData> toRemove = null;
                spawnOptions[i].Difficulty = GlobalConfig.DifficultyText(i);
                foreach (var option in spawnOptions[i].Options)
                {
                    if (option == null)
                    {
                        Debug.LogWarning($"{GlobalConfig.DifficultyText(i)} difficulty spawn options on {transform} contain empty ships!");
                        continue;
                    }
                    var a = option as PlayerData;
                    if (a != null)
                    {
                        Debug.LogError($"{GlobalConfig.DifficultyText(i)} difficulty spawn options on {transform} contain player ships: {a}! Removing!");
                        toRemove ??= new();
                        toRemove.Add(a);
                    }
                }
                if (!toRemove.IsEmpty())
                {
                    foreach (var a in toRemove)
                    {
                        spawnOptions[i].Options.Remove(a);
                    }
                }
            }

        }

        public void OnDrawGizmos()
        {
            Gizmos.color = team == GlobalConfig.PlayerTeam ? Color.blue : Color.red;
            Gizmos.DrawWireSphere(transform.position, Mathf.Max(spawnRadius, 1));
        }

        public Unit Spawn()
        {
            var spawnOption = spawnOptions[Mathf.Min(spawnOptions.Count - 1, GameManager.Difficulty)];
            return SpawnUnit(spawnOption.Options[Random.Range(0, spawnOption.Options.Count)]);
        }

        protected Unit SpawnUnit(UnitData data)
        {
            if (data == null)
            {
                Debug.LogError($"{transform.name} has no spawn data!");
                return null;
            }
            var unit = data.CreateInstance();
            unit.Team = team;
            unit.PlaySpawnEffect = PlaySpawnEffect;
            unit.CriticalObjective = criticalObjective;
            //Debugging.Log($"Team of {unit.Transform} is {unit.Team}.");
            unit.Transform.SetPositionAndRotation(transform.position +
                Random.insideUnitSphere * spawnRadius, transform.rotation);
            return unit;
        }
    }
}