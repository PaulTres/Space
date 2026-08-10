using Player;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Missions/Handmade battle", fileName = "HandmadeBattle")]
public class HandmadeBattle : Mission
{
    [SerializeField] protected Vector3 playerSpawnPos, playerSpawnRot;
    [SerializeField] protected List<UnitSpawnData> spawns = new();

    public override void FinishInitialization()
    {
        UISystem.ShowText(new(Vector3.zero, 3, "Eliminate all targets", Color.red));
    }

    public override void SpawnPlayer()
    {
        PlayerPoints -= UnitData.PlayerChoice.Points;
        SpawnUnit(UnitData.PlayerChoice, playerSpawnPos, Quaternion.Euler(playerSpawnRot),
            GlobalConfig.PlayerTeam);
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
                choice = spawner.ChooseUnit(allyPoints);
                allyPoints -= choice.Points;
            }
            else if (spawner.Team == GlobalConfig.EnemyTeam)
            {
                choice = spawner.ChooseUnit(enemyPoints);
                enemyPoints -= choice.Points;
            }
            else choice = spawner.ChooseUnit(int.MaxValue);

            SpawnUnit(choice, spawner.SpawnPosition + Random.insideUnitSphere *
                spawner.SpawnRadius, Quaternion.Euler(spawner.SpawnRotation),
                spawner.Team);
        }
    }
}
