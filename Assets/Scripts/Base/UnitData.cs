using Spawning;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "ScriptableObjects/UnitData", fileName = "UnitData")]
public class UnitData : MonoSpawnableData<Unit>
{
    public static int PlayerChoiceIndex { get; set; } = 0;
    public static List<UnitData> AvailablePlayerOptions { get; } = new();
    public static UnitData PlayerChoice => AvailablePlayerOptions[PlayerChoiceIndex];
    public static List<UnitData> AvailableAllyUnits { get; } = new();
    public static List<UnitData> AvailableEnemyUnits { get; } = new();

    public static async Task LoadAllSpawnableUnits()
    {
        var playerHandle = Addressables.LoadAssetsAsync<UnitData>(GlobalConfig.
            PlayerOptionsAddressableTag, (data) => AvailablePlayerOptions.Add(data));
        var friendHandle = Addressables.LoadAssetsAsync<UnitData>(GlobalConfig.
            AllyUnitsAddressableTag, (data) => AvailableAllyUnits.Add(data));
        var enemyHandle = Addressables.LoadAssetsAsync<UnitData>(GlobalConfig.
            EnemyUnitsAddressableTag, (data) => AvailableEnemyUnits.Add(data));

        await Task.WhenAll(playerHandle.Task, friendHandle.Task, enemyHandle.Task);
    }

    public float Points = 1;
    public string Description;
}
