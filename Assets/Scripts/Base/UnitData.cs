using Spawning;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/UnitData", fileName = "UnitData")]
public class UnitData : MonoSpawnableData<Unit>
{
    public static readonly List<UnitData> AvailableAllyUnits = new();
    public static readonly List<UnitData> AvailableEnemyUnits = new();

    public float Points = 1;
    public string Description;
}
