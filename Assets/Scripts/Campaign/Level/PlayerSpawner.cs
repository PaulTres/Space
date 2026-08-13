using UnityEngine;

namespace Level
{
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] protected float delay = 0;
        [SerializeField] protected bool playSpawnEffect = true;

        protected void Awake()
        {
            MissionManager.TryGetInstance(true).ExecuteAction(SpawnPlayer, delay);
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1);
        }
        public void SpawnPlayer()
        {
            var unit = UnitData.PlayerChoice.CreateInstance();
            unit.Team = GlobalConfig.PlayerTeam;
            unit.PlaySpawnEffect = playSpawnEffect;
            //Debug.Log($"Team of {unit.Transform} is {unit.Team}.");
            unit.Transform.SetPositionAndRotation(transform.position, transform.rotation);
            unit.gameObject.SetActive(true);
        }
    }
}