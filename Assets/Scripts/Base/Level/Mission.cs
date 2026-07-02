using EventBus;
using Spawning;
using UnityEngine;
using Utilities;
public abstract class Mission
{
    public static Mission CurrentMission { get; set; }
    protected float startTime;

    public void CheckIfMissionIsOver()
    {
        if (IsMissionOver()) MissionManager.TryGetInstance().EndMission();
    }

    public virtual void OnUnitDestroyed(ObjectDestroyed @event)
    {
        EventBus<ObjectDestroyed>.RemoveActions(@event.VictimID, OnUnitDestroyed);
        GameManager.UnitsDestroyed.Add(@event);
    }

    protected virtual Unit SpawnUnit(MonoSpawnableData<Unit> data, int team)
    {
        var unit = data.CreateInstance();
        unit.Team = team;
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetInstanceID(unit), OnUnitDestroyed);
        return unit;
    }

    public static Mission GetRandomMission() => null;

    public virtual void Initialize()
    {
        startTime = Time.time;
    }

    public abstract bool IsMissionOver();
}

public class FleetBattleMission : Mission
{
    public float RemainingTime => TimeLimit - startTime;
    public float TimeLimit { get; protected set; }
    public int EnemyUnitsCount { get; protected set; }
    public int AllyUnitsCount { get; protected set; }
    protected int playerPoints, enemyPoints;
    protected Vector3 playerFleetSpawn, enemyFleetSpawn;

    public FleetBattleMission(float timeLimit, int playerPoints, int enemyPoints, Vector3 playerFleetSpawn, Vector3 enemyFleetSpawn)
    {
        TimeLimit = timeLimit;
        this.playerPoints = playerPoints;
        this.enemyPoints = enemyPoints;
        this.playerFleetSpawn = playerFleetSpawn;
        this.enemyFleetSpawn = enemyFleetSpawn;
    }

    public override void Initialize()
    {
        base.Initialize();

    }

    protected override Unit SpawnUnit(MonoSpawnableData<Unit> data, int team)
    {
        if (team == GlobalConfig.PlayerTeam) AllyUnitsCount++;
        else if (team == GlobalConfig.EnemyTeam) EnemyUnitsCount++;
        return base.SpawnUnit(data, team);
    }

    public override void OnUnitDestroyed(ObjectDestroyed @event)
    {
        base.OnUnitDestroyed(@event);
        if (@event.VictimTeam == GlobalConfig.PlayerTeam)
        {
            AllyUnitsCount--;
            if (AllyUnitsCount == 0) CheckIfMissionIsOver();
        }
        else if (@event.VictimTeam == GlobalConfig.EnemyTeam)
        {
            EnemyUnitsCount--;
            if (EnemyUnitsCount == 0) CheckIfMissionIsOver();
        }
    }

    public override bool IsMissionOver()
    {
        return (TimeLimit <= 0 || AllyUnitsCount <= 0 || EnemyUnitsCount <= 0);
    }
}