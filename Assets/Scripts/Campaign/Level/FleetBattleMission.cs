using System.Collections.Generic;
using UnityEngine;

public class FleetBattleMission : Mission
{
    public float RemainingTime => TimeLimit - startTime;
    public float TimeLimit { get; protected set; }
    public int EnemyUnitsCount { get; protected set; }
    public int AllyUnitsCount { get; protected set; }
    protected Vector3 playerSpawn, enemySpawn;
    protected float distanceFromCenter, playerSpawnRadius, enemySpawnRadius;

    public FleetBattleMission(float timeLimit, int playerPoints, int enemyPoints) : base(playerPoints, enemyPoints)
    {
        TimeLimit = timeLimit;
        distanceFromCenter = (playerPoints + enemyPoints +
            GlobalConfig.ShipSpawnEmptySphereRadius) * 2;
        playerSpawnRadius = playerPoints;
        enemySpawnRadius = enemyPoints;
    }

    public override void Initialize()
    {
        base.Initialize();
        SpawnAllies();
        SpawnEnemies();
    }

    protected void SpawnAllies()
    {
        var allyRotation = Quaternion.identity;
        var allySpawnPos = distanceFromCenter * Vector3.forward;
        int retries = 0;
        List<UnitData> allyOptions = new() { UnitData.PlayerChoice };
        while (PlayerPoints > 0)
        {
            var chosenAlly = UnitData.AvailableAllyUnits[Random.Range(0,
                UnitData.AvailableAllyUnits.Count)];
            if (chosenAlly.Points > PlayerPoints)
            {
                retries++;
                if (retries >= GlobalConfig.MaxSpawnRetries)
                {
                    bool found = false;
                    foreach (var ally in UnitData.AvailableAllyUnits)
                    {
                        if (ally.Points <= PlayerPoints)
                        {
                            found = true;
                            chosenAlly = ally;
                            break;
                        }
                    }
                    if (!found) break;
                }
                else continue;
            }
            retries = 0;
            EnemyPoints -= chosenAlly.Points;
            allyOptions.Add(chosenAlly);
        }

        //spawn the enemies on a grid, to prevent collisions
        var gridSize = Mathf.Pow(allyOptions.Count, 1f / 3f + 1);
        var listIndex = 0;
        for (int i = 0; i < gridSize && listIndex < allyOptions.Count; i++)
        {
            for (int j = 0; j < gridSize && listIndex < allyOptions.Count; j++)
            {
                for (int h = 0; h < gridSize && listIndex < allyOptions.Count; h++)
                {
                    var spawnPos = allySpawnPos + new Vector3(
                        i * GlobalConfig.ShipSpawnEmptySphereRadius,
                        j * GlobalConfig.ShipSpawnEmptySphereRadius,
                        h * GlobalConfig.ShipSpawnEmptySphereRadius);
                    SpawnUnit(allyOptions[listIndex], GlobalConfig.PlayerTeam, spawnPos,
                        allyRotation);
                    listIndex++;
                }
            }
        }
    }

    protected void SpawnEnemies()
    {
        var enemyRotation = Quaternion.Euler(new Vector3(0, 180, 0));
        //pick a position for the bad guys
        var enemySpawnPos = distanceFromCenter * Vector3.forward;
        int retries = 0;
        List<UnitData> enemyOptions = new();
        while (EnemyPoints > 0)
        {
            var chosenEnemy = UnitData.AvailableEnemyUnits[Random.Range(0,
                UnitData.AvailableEnemyUnits.Count)];
            if (chosenEnemy.Points > EnemyPoints)
            {
                retries++;
                if (retries >= GlobalConfig.MaxSpawnRetries)
                {
                    bool found = false;
                    foreach (var enemy in UnitData.AvailableEnemyUnits)
                    {
                        if (enemy.Points <= EnemyPoints)
                        {
                            found = true;
                            chosenEnemy = enemy;
                            break;
                        }
                    }
                    if (!found) break;
                }
                else continue;
            }
            retries = 0;
            EnemyPoints -= chosenEnemy.Points;
            enemyOptions.Add(chosenEnemy);
        }

        //spawn the enemies on a grid, to prevent collisions
        var gridSize = Mathf.Pow(enemyOptions.Count, 1f / 3f + 1);
        var listIndex = 0;
        for (int i = 0; i < gridSize && listIndex < enemyOptions.Count; i++)
        {
            for (int j = 0; j < gridSize && listIndex < enemyOptions.Count; j++)
            {
                for (int h = 0; h < gridSize && listIndex < enemyOptions.Count; h++)
                {
                    var spawnPos = enemySpawnPos + new Vector3(
                        i * GlobalConfig.ShipSpawnEmptySphereRadius,
                        j * GlobalConfig.ShipSpawnEmptySphereRadius,
                        h * GlobalConfig.ShipSpawnEmptySphereRadius);
                    SpawnUnit(enemyOptions[listIndex], GlobalConfig.EnemyTeam, spawnPos,
                        enemyRotation);
                    listIndex++;
                }
            }
        }
    }

    protected override Unit SpawnUnit(UnitData data, int team, Vector3 position, Quaternion rotation)
    {
        if (team == GlobalConfig.PlayerTeam) AllyUnitsCount++;
        else if (team == GlobalConfig.EnemyTeam) EnemyUnitsCount++;
        return base.SpawnUnit(data, team, position, rotation);
    }

    public override void OnUnitDestroyed(ObjectDestroyed @event)
    {
        base.OnUnitDestroyed(@event);
        if (GameManager.Player == null || @event.VictimID == GameManager.Player.GetInstanceID())
        {
            MissionManager.TryGetInstance().EndMission();
            return;
        }
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
        return (TimeLimit <= 0 || GameManager.Player == null || AllyUnitsCount <= 0 ||
            EnemyUnitsCount <= 0);
    }
}