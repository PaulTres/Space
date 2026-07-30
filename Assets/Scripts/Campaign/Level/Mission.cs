using Player;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public abstract class Mission : ScriptableObject
{
    #region Static stuff
    public static Mission CurrentMission { get; set; }
    public static List<Mission> Campaign { get; } = new();

    public static async Task LoadCampaignMissions()
    {
        Campaign.Clear();
        var handle = Addressables.LoadAssetAsync<Campaign>("Campaign");
        var instance = await handle.Task;
        foreach (var a in instance.Missions)
        {
            Campaign.Add(a);
        }
    }
    #endregion

    public float StartTime { get; protected set; }
    /// <summary>
    /// Time limit for the mission. Ignored if set to <=0.
    /// </summary>
    [field: SerializeField] public float TimeLimit { get; protected set; } = -1;
    [field: SerializeField] public string Briefing { get; set; } = "No briefing exists.";
    [field: SerializeField] public int PlayerPoints { get; protected set; } = int.MaxValue;
    [field: SerializeField] public int EnemyPoints { get; protected set; } = int.MaxValue;
    [SerializeField] protected Material skyboxMaterial;

    public void EndMission()
    {
        var m = MissionManager.TryGetInstance();
        if (m != null) m.EndMission();
    }

    /// <summary>
    /// Fires when the mission manager is initialized. Initializes time.
    /// Should handle environment, spawning etc.
    /// </summary>
    public virtual void Initialize()
    {
        RenderSettings.skybox = skyboxMaterial;
        DynamicGI.UpdateEnvironment();
        CurrentMission = this;
        StartTime = Time.time;
        //Debug.Log("Starting mission");
        UnitManager.Clear();
        GameManager.UnitsDestroyed.Clear();
        SpawnPlayer();
        SpawnScenery();
        SpawnUnits();
        FinishInitialization();
    }

    public abstract void FinishInitialization();

    public abstract void SpawnPlayer();

    public virtual void SpawnScenery() { }

    public abstract void SpawnUnits();

    public virtual void OnAllEnemiesDestroyed()
    {
        //Debug.Log("enemies destroyed");
        if (GameManager.Player != null)
        {
            UISystem.ShowText(new(Vector3.zero, 5, "Victory!", Color.green));
            EndMission();
        }
    }
}
