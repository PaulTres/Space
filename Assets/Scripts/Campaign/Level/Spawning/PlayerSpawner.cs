using Player;
using UnityEngine;
using UnityEngine.Events;

namespace Level
{
    [RequireComponent(typeof(OSTTrigger))]
    public class PlayerSpawner : MonoBehaviour
    {
        [SerializeField] protected float delay = 0;
        [SerializeField] protected bool playSpawnEffect = true;
        [SerializeField] protected UnityEvent onPlayerSpawned;

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
            unit.name = "You";
            unit.Transform.SetPositionAndRotation(transform.position, transform.rotation);
            unit.gameObject.SetActive(true);
            GetComponent<OSTTrigger>().SetOST();
            onPlayerSpawned?.Invoke();
        }
    }
}