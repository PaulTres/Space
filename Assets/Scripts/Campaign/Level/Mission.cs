using EventBus;
using UnityEngine;
using Utilities;

public abstract class Mission
{
    public static Mission CurrentMission { get; set; }
    protected float startTime;
    public int PlayerPoints { get; protected set; }
    public int EnemyPoints { get; protected set; }
    public string Briefing { get; set; } = "";
    public Mission(int playerPoints, int enemyPoints)
    {
        PlayerPoints = playerPoints;
        EnemyPoints = enemyPoints;
    }

    public void CheckIfMissionIsOver()
    {
        if (IsMissionOver()) MissionManager.TryGetInstance().EndMission();
    }

    public virtual void OnUnitDestroyed(ObjectDestroyed @event)
    {
        EventBus<ObjectDestroyed>.RemoveActions(@event.VictimID, OnUnitDestroyed);
        GameManager.UnitsDestroyed.Add(@event);
    }

    protected Unit SpawnUnit(UnitData unit, int team, Transform transform)
    {
        return SpawnUnit(unit, team, transform.position, transform.rotation);
    }

    protected Unit SpawnUnit(UnitData unit, int team, Vector3 position)
    {
        return SpawnUnit(unit, team, position, Quaternion.identity);
    }

    protected virtual Unit SpawnUnit(UnitData data, int team, Vector3 position, Quaternion rotation)
    {
        var unit = data.CreateInstance();
        unit.Team = team;
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetInstanceID(unit), OnUnitDestroyed);
        unit.transform.root.SetPositionAndRotation(position, rotation);
        return unit;
    }

    public static Mission GetRandomMission() => null;

    public virtual void Initialize()
    {
        startTime = Time.time;
    }

    public abstract bool IsMissionOver();
}
