using Player;
using UnityEngine;

namespace Level
{
    [CreateAssetMenu(menuName = "Missions/Handmade battle", fileName = "HandmadeBattle")]
    public class HandmadeBattle : Mission
    {
        [SerializeField] protected Vector3 playerSpawnPos, playerSpawnRot;
        [SerializeField] protected UnitWave enemyWave, allyWave;

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
            allyWave.Spawn(PlayerPoints);
            enemyWave.OnAllUnitsDestroyed += () => MissionManager.TryGetInstance(true).MissionVictory();
            enemyWave.Spawn(EnemyPoints);
        }
    }
}