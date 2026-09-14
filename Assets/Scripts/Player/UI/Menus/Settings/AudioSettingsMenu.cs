using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
namespace Player
{

    public class AudioSettingsMenu : MonoBehaviour
    {
        [SerializeField] protected Slider masterSlider;
        [SerializeField] protected Slider musicSlider;
        [SerializeField] protected Slider sfxSlider;

        protected const string mixerMaster = "MasterVolume";
        protected const string mixerMusic = "MusicVolume";
        protected const string mixerSFX = "SFXVolume";

        protected const string keyMaster = "Audio_Master";
        protected const string keyMusic = "Audio_Music";
        protected const string keySFX = "Audio_SFX";

        [SerializeField] AudioMixer audioMixer;

        protected void OnEnable()
        {
            float master = PlayerPrefs.GetFloat(keyMaster, 1f);
            float music = PlayerPrefs.GetFloat(keyMusic, 1f);
            float sfx = PlayerPrefs.GetFloat(keySFX, 1f);

            masterSlider.SetValueWithoutNotify(master);
            musicSlider.SetValueWithoutNotify(music);
            sfxSlider.SetValueWithoutNotify(sfx);

            ApplyVolume(mixerMaster, master);
            ApplyVolume(mixerMusic, music);
            ApplyVolume(mixerSFX, sfx);
        }

        protected void OnDisable()
        {
            SaveAudioSettings();
        }

        public void SetMasterVolume(float value) => ApplyVolume(mixerMaster, value);

        public void SetMusicVolume(float value) => ApplyVolume(mixerMusic, value);

        public void SetSFXVolume(float value) => ApplyVolume(mixerSFX, value);

        protected void ApplyVolume(string parameter, float linearValue)
        {
            float db = Mathf.Log10(Mathf.Max(linearValue, 0.0001f)) * 20f;
            audioMixer.SetFloat(parameter, db);
        }

        public void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat(keyMaster, masterSlider.value);
            PlayerPrefs.SetFloat(keyMusic, musicSlider.value);
            PlayerPrefs.SetFloat(keySFX, sfxSlider.value);
            PlayerPrefs.Save();
        }

        public static void LoadAudioSettings(AudioMixer mixer)
        {
            SetMixerVolume(mixer, mixerMaster, PlayerPrefs.GetFloat(keyMaster, 1f));
            SetMixerVolume(mixer, mixerMusic, PlayerPrefs.GetFloat(keyMusic, 1f));
            SetMixerVolume(mixer, mixerSFX, PlayerPrefs.GetFloat(keySFX, 1f));
        }

        static void SetMixerVolume(AudioMixer mixer, string parameter, float linearValue)
        {
            float db = Mathf.Log10(Mathf.Max(linearValue, 0.0001f)) * 20f;
            mixer.SetFloat(parameter, db);
        }
    }
}