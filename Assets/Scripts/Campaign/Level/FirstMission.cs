using Player;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
    [CreateAssetMenu(menuName = "Missions/Mission1", fileName = "Mission1")]
    public class FirstMission : Mission
    {
        [SerializeField] protected UnitSpawnData friendly;
        [SerializeField] protected List<UnitWave> enemyWaves;
        protected int currentWave = 0;

        public override void SpawnPlayer()
        {
            SpawnUnit(UnitData.PlayerChoice, Vector3.zero, Quaternion.identity, GlobalConfig.PlayerTeam);
        }

        public void WaveDestroyed()
        {
            //Debug.Log("Wave destroyed");
            if (currentWave >= enemyWaves.Count - 1)
            {
                MissionManager.TryGetInstance(true).MissionVictory();
            }
            else
            {
                currentWave++;
                MissionManager.TryGetInstance(true).ExecuteAction(() => enemyWaves[currentWave].Spawn(), 5);
            }
        }

        public override void SpawnUnits()
        {
            var choice = friendly.ChooseUnit(int.MaxValue);
            SpawnUnit(choice, friendly.SpawnPosition + UnityEngine.Random.insideUnitSphere *
                  friendly.SpawnRadius, Quaternion.Euler(friendly.SpawnRotation), friendly.Team);

            currentWave = 0;
            for (int i = 0; i < enemyWaves.Count; i++)
            {
                enemyWaves[i].OnAllUnitsDestroyed += WaveDestroyed;
            }
            MissionManager.TryGetInstance(true).ExecuteAction(() => enemyWaves[currentWave].Spawn(), 5);
        }

        public override void FinishInitialization()
        {
            UISystem.ShowText(new(Vector3.zero, 3, "Eliminate all targets", Color.red));
        }
    }
}