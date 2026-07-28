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
        var player = UnitData.PlayerChoice.CreateInstance();
        player.Transform.SetPositionAndRotation(playerSpawnPos, Quaternion.
            Euler(playerSpawnRot));
        PlayerPoints -= UnitData.PlayerChoice.Points;
        player.gameObject.SetActive(true);
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
            var unit = choice.CreateInstance();
            unit.Team = spawner.Team;
            //Debug.Log($"Team of {unit.Transform} is {unit.Team}.");
            unit.Transform.SetPositionAndRotation(pos, Quaternion.
                Euler(spawner.SpawnRotation));
            unit.gameObject.SetActive(true);
        }
    }
}
