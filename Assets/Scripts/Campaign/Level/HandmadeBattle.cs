using System.Collections.Generic;
using UnityEngine;

public class HandmadeBattle : Mission
{
    [SerializeField] protected Vector3 playerSpawnPos, playerSpawnRot;
    [SerializeField] protected List<UnitSpawnData> spawns = new();

    public override void SpawnPlayer()
    {
        var player = UnitData.PlayerChoice.CreateInstance();
        player.Transform.SetPositionAndRotation(playerSpawnPos, Quaternion.
            Euler(playerSpawnRot));
        player.gameObject.SetActive(true);
        PlayerPoints -= UnitData.PlayerChoice.Points;
    }

    public override void SpawnUnits()
    {
        var allyPoints = PlayerPoints;
        var enemyPoints = EnemyPoints;
        foreach (var spawner in spawns)
        {
            UnitData choice = null;
            if (spawner.Team == GlobalConfig.PlayerTeam)
            {
                choice = spawner.SpawnUnit(allyPoints);
                allyPoints -= choice.Points;
            }
            else if (spawner.Team == GlobalConfig.EnemyTeam)
            {
                choice = spawner.SpawnUnit(enemyPoints);
                enemyPoints -= choice.Points;
            }
            else choice = spawner.SpawnUnit(int.MaxValue);
            var pos = spawner.SpawnPosition + Random.insideUnitSphere *
                spawner.SpawnRadius;
            choice.CreateInstance().Transform.SetPositionAndRotation(pos,
                Quaternion.Euler(spawner.SpawnRotation));
        }
    }
}
