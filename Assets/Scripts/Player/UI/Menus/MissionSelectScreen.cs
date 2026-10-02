using Level;
using TMPro;
using UnityEngine;
using Utilities;
namespace Player
{
    public class MissionSelectScreen : MonoBehaviour
    {
        [SerializeField] protected TextMeshProUGUI missionBriefingText;
        [SerializeField] protected TMP_Dropdown missionOptions;

        #region Setup
        protected void OnEnable()
        {
            PopulateMissionDropdown();
        }

        protected void PopulateMissionDropdown()
        {
            missionOptions.ClearOptions();
            var latestMission = Mathf.Min(Mission.Campaign.Count - 1, GameManager.LatestMissionIndex);
            for (int i = 0; i <= latestMission; i++)
            {
                //Debug.Log(i);
                missionOptions.options.Add(new(Mission.Campaign[i].name));
            }
            missionOptions.SetValueWithoutNotify(latestMission);
            Mission.CurrentMission = Mission.Campaign[latestMission];
            GameManager.ChosenMissionIndex = latestMission;
            missionBriefingText.text = Mission.CurrentMission.Briefing;
        }

        #endregion

        public void SetMissionChoice(int index)
        {
            GlobalConfig.ClickSound.Play(transform.position, requiresPlayer: false);
            Mission.CurrentMission = Mission.Campaign[index];
            GameManager.ChosenMissionIndex = index;
            missionBriefingText.text = Mission.CurrentMission.Briefing;
        }

        public void BackToMenu() => GameManager.LoadMainMenu();
    }
}