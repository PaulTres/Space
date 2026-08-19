using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Level
{
    [CreateAssetMenu(menuName = "Missions/Mission", fileName = "Mission")]
    public class Mission : ScriptableObject
    {
        #region Static stuff
        public static Mission CurrentMission { get; set; }
        public static List<Mission> Campaign { get; } = new();

        public static async Task LoadCampaignMissions()
        {
            Campaign.Clear();
            var handle = Addressables.LoadAssetAsync<Campaign>("Campaign");
            var instance = await handle.Task;
            foreach (var a in instance.Missions)
            {
                Campaign.Add(a);
            }
        }
        #endregion

        [Tooltip("Time limit for the mission. Ignored if set to <= 0.")]
        [field: SerializeField] public float TimeLimit { get; protected set; } = -1;
        [SerializeField][TextArea(15, 20)] protected string briefing = "No briefing exists.";
        [SerializeField][TextArea(15, 20)] protected string afterActionReport = "All quiet on the gay front.";
        public string Briefing => briefing;
        public string AfterActionReport => afterActionReport;
        [field: SerializeField] public int PlayerPoints { get; protected set; } = int.MaxValue;
        [field: SerializeField] public int EnemyPoints { get; protected set; } = int.MaxValue;
    }
}