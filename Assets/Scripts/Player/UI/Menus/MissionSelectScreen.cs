using Level;
using TMPro;
using UnityEngine;
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
            for (int i = 0; i <= GameManager.LatestMissionIndex; i++)
            {
                //Debug.Log(i);
                missionOptions.options.Add(new(Mission.Campaign[i].name));
            }
            missionOptions.SetValueWithoutNotify(Mathf.Min(GameManager.LatestMissionIndex, missionOptions.options.Count - 1));
            SetMissionChoice(Mathf.Min(GameManager.LatestMissionIndex, missionOptions.options.Count - 1));
        }

        #endregion

        public void SetMissionChoice(int index)
        {
            Mission.CurrentMission = Mission.Campaign[index];
            GameManager.ChosenMissionIndex = index;
            missionBriefingText.text = Mission.CurrentMission.Briefing;
        }

        public void BackToMenu() => GameManager.LoadMainMenu();

        public void GoToShipSelect() => GameManager.LoadShipSelect();
    }
}