using Level;
using System.Collections;
using TMPro;
using UnityEngine;
namespace Player
{
    public class ShipSelectScreen : MonoBehaviour
    {
        [SerializeField] protected int missionStartTimerDuration = 5;
        [SerializeField] protected TextMeshProUGUI timerText, shipDescriptionText;
        [SerializeField] protected TMP_Dropdown playerOptions;
        protected WaitForSeconds waitForOneSecond = new(1);
        protected Coroutine coroutine;

        #region Setup
        protected void OnEnable()
        {
            timerText.text = $"00:{missionStartTimerDuration:D2}";
            PopulatePlayerChoiceDropdown();
        }

        protected void PopulatePlayerChoiceDropdown()
        {
            playerOptions.ClearOptions();
            foreach (var data in UnitData.AvailablePlayerOptions)
            {
                if (data.Points > Mission.CurrentMission.PlayerPoints) continue;
                if (data.MissionRequirement >= GameManager.LatestMissionIndex) continue;
                playerOptions.options.Add(new(data.name));
            }
            SetPlayerChoice(0);
            playerOptions.SetValueWithoutNotify(0);
        }

        protected void OnDisable()
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        #endregion

        public void SetPlayerChoice(int index)
        {
            UnitData.PlayerChoiceIndex = index;
            shipDescriptionText.text = "Ship description:\n" + UnitData.AvailablePlayerOptions[index].Description;
        }

        public void GoToMissionSelect() => GameManager.LoadMissionSelection();

        #region Mission start
        public void BeginMission()
        {
            coroutine ??= StartCoroutine(MissionStartCountdown());
        }

        protected IEnumerator MissionStartCountdown()
        {
            int count = missionStartTimerDuration;
            while (count > 1)
            {
                count--;
                timerText.text = $"00:{count:D2}";
                yield return waitForOneSecond;
            }
            GameManager.LoadLoadingScreen();
        }
        #endregion
    }
}