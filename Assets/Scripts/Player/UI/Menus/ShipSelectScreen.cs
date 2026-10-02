using Level;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Player
{
    public class ShipSelectScreen : MonoBehaviour
    {
        [SerializeField] protected int missionStartTimerDuration = 5;
        [SerializeField] protected TextMeshProUGUI timerText;
        [SerializeField] protected float fadeOutDuration = 1;
        [SerializeField] protected TMP_Dropdown playerOptions;
        [Header("Current ship info")]
        [SerializeField] protected TextMeshProUGUI shipDescriptionText;
        [SerializeField] protected Transform previewModelAzimuth, previewModelRoll;
        [SerializeField] protected Slider azimuthSlider, pitchSlider;
        [SerializeField] protected SceneFadeInAndOut sceneFadeScript;
        protected List<PlayerData> playerOptionsList;
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
            playerOptionsList = new();
            UnitData.AvailablePlayerOptions.Sort((u1, u2) => u2.MissionRequirement.CompareTo(u1.MissionRequirement));
            foreach (var data in UnitData.AvailablePlayerOptions)
            {
                if (data.Points > Mission.CurrentMission.PlayerPoints) continue;
                //Debug.Log("Considering " + data.Name + ": " + data.MissionRequirement + "; " + GameManager.LatestMissionIndex);
                if (data.MissionRequirement > GameManager.LatestMissionIndex) continue;
                playerOptionsList.Add(data);
                playerOptions.options.Add(new(data.Name));
            }
            SetPlayerChoiceNoSound(0);
            playerOptions.SetValueWithoutNotify(0);
        }

        protected void OnDisable()
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        #endregion

        public void SetPlayerChoiceNoSound(int index)
        {
            UnitData.PlayerChoice = playerOptionsList[index];
            shipDescriptionText.text = "Ship description:\n" + UnitData.PlayerChoice.Description;
            for (int i = 0; i < previewModelRoll.childCount; i++)
            {
                Destroy(previewModelRoll.GetChild(i).gameObject);
            }
            previewModelAzimuth.localRotation = Quaternion.identity;
            previewModelRoll.localRotation = Quaternion.identity;
            Instantiate(UnitData.PlayerChoice.PreviewModel, previewModelRoll);
            azimuthSlider.SetValueWithoutNotify(0);
            pitchSlider.SetValueWithoutNotify(0);
        }

        public void SetPlayerChoice(int index)
        {
            GlobalConfig.ClickSound.Play(transform.position, requiresPlayer: false);
            UnitData.PlayerChoice = playerOptionsList[index];
            shipDescriptionText.text = "Ship description:\n" + UnitData.PlayerChoice.Description;
            for (int i = 0; i < previewModelRoll.childCount; i++)
            {
                Destroy(previewModelRoll.GetChild(i).gameObject);
            }
            previewModelAzimuth.localRotation = Quaternion.identity;
            previewModelRoll.localRotation = Quaternion.identity;
            Instantiate(UnitData.PlayerChoice.PreviewModel, previewModelRoll);
            azimuthSlider.SetValueWithoutNotify(0);
            pitchSlider.SetValueWithoutNotify(0);
        }

        public void ChangeModelAzimuth(float value)
        {
            previewModelAzimuth.localRotation = Quaternion.Euler(0, value, 0);
        }

        public void ChangeModelPitch(float value)
        {
            previewModelRoll.localRotation = Quaternion.Euler(0, 0, value);
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
            sceneFadeScript.FadeOut(() => GameManager.LoadMission());
        }
        #endregion
    }
}