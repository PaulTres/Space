using System.Collections;
using TMPro;
using UnityEngine;
namespace Player
{
    public class LobbyScreen : MonoBehaviour
    {
        [SerializeField] protected UIInputReader inputReader;
        [SerializeField] protected int missionStartTimerDuration = 5;
        [SerializeField] protected TextMeshProUGUI timerText, shipDescriptionText, missionBriefingText;
        [SerializeField] protected TMP_Dropdown playerOptions, missionOptions;
        [SerializeField] protected GameObject pauseMenu;
        protected WaitForSeconds waitForOneSecond = new(1);
        protected Coroutine coroutine;

        #region Setup
        protected void OnEnable()
        {
            PopulateMissionDropdown();
            inputReader.EnablePlayerActions();
            inputReader.OnEscapeEvent += OnEscape;
            timerText.text = $"Start: {missionStartTimerDuration}";
            pauseMenu.SetActive(false);
        }

        protected void PopulateMissionDropdown()
        {
            missionOptions.ClearOptions();
            for (int i = 0; i <= GameManager.LatestMissionIndex; i++)
            {
                //Debug.Log(i);
                missionOptions.options.Add(new(Mission.Campaign[i].name));
            }
            missionOptions.SetValueWithoutNotify(GameManager.LatestMissionIndex);
            SetMissionChoice(GameManager.LatestMissionIndex);
        }

        public void SetMissionChoice(int index)
        {
            Mission.CurrentMission = Mission.Campaign[index];
            GameManager.ChosenMissionIndex = index;
            missionBriefingText.text = Mission.CurrentMission.Briefing;
            PopulatePlayerChoiceDropdown();
        }

        protected void PopulatePlayerChoiceDropdown()
        {
            playerOptions.ClearOptions();
            foreach (var data in UnitData.AvailablePlayerOptions)
            {
                if (data.Points > Mission.CurrentMission.PlayerPoints) continue;
                playerOptions.options.Add(new(data.name));
            }
            SetPlayerChoice(0);
            playerOptions.SetValueWithoutNotify(0);
        }

        protected void OnDisable()
        {
            inputReader.DisablePlayerActions();
            if (coroutine != null) StopCoroutine(coroutine);
        }
        #endregion

        protected void OnEscape()
        {
            //open the pause menu
            Debug.Log("Escaping");
            pauseMenu.SetActive(true);
        }

        public void SetPlayerChoice(int index)
        {
            UnitData.PlayerChoiceIndex = index;
            shipDescriptionText.text = UnitData.AvailablePlayerOptions[index].Description;
        }

        #region Mission start
        public void BeginMission()
        {
            //Mission.CurrentMission = Mission.GetRandomMission();
            if (coroutine == null) coroutine = StartCoroutine(MissionStartCountdown());
        }

        protected IEnumerator MissionStartCountdown()
        {
            int count = missionStartTimerDuration;
            while (count >= 0)
            {
                yield return waitForOneSecond;
                timerText.text = $"Start: {count}";
                count--;
            }
            GameManager.LoadMission();
        }
        #endregion
    }
}