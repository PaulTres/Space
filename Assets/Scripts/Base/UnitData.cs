using Spawning;
using System;
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
        try
        {
            var playerHandle = Addressables.LoadAssetsAsync<UnitData>(GlobalConfig.
                PlayerOptionsAddressableTag, (data) => AvailablePlayerOptions.Add(data));
            await playerHandle.Task;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        //try
        //{
        //    var friendHandle = Addressables.LoadAssetsAsync<UnitData>(GlobalConfig.
        //        AllyUnitsAddressableTag, (data) => AvailableAllyUnits.Add(data));
        //    await friendHandle.Task;
        //}
        //catch (Exception e)
        //{
        //    //Debug.LogException(e);
        //}

        //try
        //{
        //    var enemyHandle = Addressables.LoadAssetsAsync<UnitData>(GlobalConfig.
        //    EnemyUnitsAddressableTag, (data) => AvailableEnemyUnits.Add(data));
        //    await enemyHandle.Task;
        //}
        //catch (Exception e)
        //{
        //    //Debug.LogException(e);
        //}
    }

    public int Points = 1;
    [SerializeField] protected string description;
    public string Description
    {
        get
        {
            if (description != null && description.Length > 0) return description;
            return "A " + name + '.';
        }
    }
}
