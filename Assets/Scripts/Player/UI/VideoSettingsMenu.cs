using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Player
{
    public class VideoSettingsMenu : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI qualityText;
        [SerializeField] Toggle vSyncToggle, fullscreenToggle;
        [SerializeField] TMP_Dropdown resolutionDropdown;

        protected Resolution[] resolutions;

        public const string KeyQuality = "QualityLevel", KeyVSync = "VSync", KeyFullscreen = "Fullscreen";

        protected void OnEnable()
        {
            UpdateQualityText();
            vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
            fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            PopulateResolutions();
        }

        protected void OnDisable()
        {
            SaveVideoSettings();
        }

        protected void UpdateQualityText()
        {
            var q = QualitySettings.GetQualityLevel();
            qualityText.text = QualitySettings.names[q];
        }

        public void IncreaseLevel()
        {
            QualitySettings.IncreaseLevel(true);
            UpdateQualityText();
        }

        public void DecreaseLevel()
        {
            QualitySettings.DecreaseLevel(true);
            UpdateQualityText();
        }

        public void ToggleVSync(bool isOn) => QualitySettings.vSyncCount = isOn ? 1 : 0;

        public void SetFullscreen(bool isOn) => Screen.fullScreen = isOn;

        public void SetResolution(int index)
        {
            var r = resolutions[index];
            Screen.SetResolution(r.width, r.height, Screen.fullScreenMode, r.refreshRateRatio);
        }

        protected void PopulateResolutions()
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            var options = new List<string>();
            int currentIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                options.Add($"{resolutions[i].width} x {resolutions[i].height} @ " +
                    $"{resolutions[i].refreshRateRatio.value:F0}Hz");
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                    currentIndex = i;
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.SetValueWithoutNotify(currentIndex);
        }

        public void SaveVideoSettings()
        {
            PlayerPrefs.SetInt(KeyQuality, QualitySettings.GetQualityLevel());
            PlayerPrefs.SetInt(KeyVSync, QualitySettings.vSyncCount);
            PlayerPrefs.SetInt(KeyFullscreen, Screen.fullScreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void LoadVideoSettings()
        {
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.SetQualityLevel(PlayerPrefs.GetInt(KeyQuality,
                QualitySettings.names.Length - 1), true);
            QualitySettings.vSyncCount = PlayerPrefs.GetInt(KeyVSync, 0);
            Screen.fullScreen = PlayerPrefs.GetInt(KeyFullscreen, 1) == 1;
        }
    }
}