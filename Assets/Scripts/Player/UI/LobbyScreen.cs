using System.Collections;
using TMPro;
using UnityEngine;
namespace Player
{
    public class LobbyScreen : MenuPDA
    {
        [SerializeField] protected int missionStartTimerDuration = 5;
        [SerializeField] protected TextMeshProUGUI timerText, shipDescriptionText;
        [SerializeField] protected TMP_Dropdown playerOptions;
        protected WaitForSeconds waitForOneSecond = new(1);
        protected Coroutine coroutine;

        #region Setup
        protected override void OnEnable()
        {
            base.OnEnable();
            PopulatePlayerChoiceDropdown();
            timerText.text = missionStartTimerDuration.ToString();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (coroutine != null) StopCoroutine(coroutine);
        }
        #endregion

        public void SetPlayerChoice(int index)
        {
            UnitData.PlayerChoiceIndex = index;
            shipDescriptionText.text = UnitData.AvailablePlayerOptions[index].Description;
        }

        protected void PopulatePlayerChoiceDropdown()
        {
            playerOptions.ClearOptions();
            foreach (var data in UnitData.AvailablePlayerOptions)
            {
                playerOptions.options.Add(new(data.name));
            }
            SetPlayerChoice(0);
            playerOptions.SetValueWithoutNotify(0);
        }

        public void BeginMission()
        {
            Mission.CurrentMission = Mission.GetRandomMission();
            coroutine = StartCoroutine(MissionStartCountdown());
        }

        protected IEnumerator MissionStartCountdown()
        {
            int count = missionStartTimerDuration;
            while (count > 0)
            {
                yield return waitForOneSecond;
                timerText.text = count.ToString();
                count--;
            }
            GameManager.LoadMission();
        }
    }
}