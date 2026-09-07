using Spawning;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "ScriptableObjects/UnitData", fileName = "UnitData")]
public class UnitData : MonoSpawnableData<Unit>
{
    #region Statics
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
            AvailablePlayerOptions.Sort((a, b) =>
            {
                if (a.MissionRequirement != b.MissionRequirement)
                {
                    return a.MissionRequirement.CompareTo(b.MissionRequirement);
                }
                return a.name.CompareTo(b.name);
            });
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
    #endregion

    public bool ShouldBeDisabled = true;
    public int Points = 1;
    [Tooltip("Only used for the player. A ship cannot be used in a mission before this one has been completed.")]
    public int MissionRequirement = -1;
    [TextArea(15, 20)]
    [SerializeField] protected string description;
    public string Name => name.Replace("(Clone)", "").Replace("_Player", "").Trim();
    public string Description
    {
        get
        {
            if (description != null && description.Length > 0) return description;
            return "A " + Name + '.';
        }
    }

    protected void OnValidate()
    {
        Debug.Assert(Prefab != null, $"Prefab is null for {this}!");
        Debug.Assert(!ShouldBeDisabled || !Prefab.gameObject.activeSelf, $"Prefab {Prefab} is active for {this}!");
    }
}
