using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class UnitSpawnData
{
    [field: SerializeField] public Vector3 SpawnPosition { get; protected set; }
    [field: SerializeField] public Vector3 SpawnRotation { get; protected set; }
    [field: SerializeField] public float SpawnRadius { get; protected set; }
    [field: SerializeField] public int Team { get; protected set; } = 1;
    [SerializeField] protected List<UnitData> spawnOptions = new();

    public UnitData SpawnUnit(int maxPoints)
    {
        var available = spawnOptions.Where(d => d.Points <= maxPoints).ToList();
        if (available.Count == 0) return null;
        var index = Random.Range(0, available.Count);
        return available[index];
    }
}
