using UnityEngine;

public abstract class Mission : ScriptableObject
{
    public static Mission CurrentMission { get; set; }
    public float StartTime { get; protected set; }
    /// <summary>
    /// Time limit for the mission. Ignored if set to <=0.
    /// </summary>
    [field: SerializeField] public float TimeLimit { get; protected set; } = -1;
    [field: SerializeField] public string Briefing { get; set; } = "No briefing exists.";
    [field: SerializeField] public int PlayerPoints { get; protected set; } = int.MaxValue;
    [field: SerializeField] public int EnemyPoints { get; protected set; } = int.MaxValue;

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
        CurrentMission = this;
        StartTime = Time.time;
        SpawnPlayer();
        SpawnScenery();
        SpawnUnits();
    }

    public abstract void SpawnPlayer();

    public virtual void SpawnScenery() { }

    public abstract void SpawnUnits();

    public virtual void OnAllEnemiesDestroyed()
    {
        EndMission();
    }
}
