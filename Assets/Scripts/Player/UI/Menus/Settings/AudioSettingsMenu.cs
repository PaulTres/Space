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
        protected static readonly string musicVolume = "MusicVolume",
            masterVolume = "MasterVolume",
            sfxVolume = "SFXVolume";
        [SerializeField] protected AudioMixer audioMixer;

        protected void OnEnable()
        {
            LoadSettings();
        }

        public static void LoadSettings(AudioMixer mixer)
        {
            var mVol = PlayerPrefs.GetFloat(masterVolume, 1);
            mixer.SetFloat(masterVolume, mVol);
            var m_vol = PlayerPrefs.GetFloat(musicVolume, 1);
            mixer.SetFloat(musicVolume, m_vol);
            var sfx = PlayerPrefs.GetFloat(sfxVolume, 1);
            mixer.SetFloat(sfxVolume, sfx);
        }

        protected void LoadSettings()
        {
            var mVol = PlayerPrefs.GetFloat(masterVolume, 1);
            masterSlider.SetValueWithoutNotify(mVol);
            SetMasterVolume(mVol);
            var m_vol = PlayerPrefs.GetFloat(musicVolume, 1);
            musicSlider.SetValueWithoutNotify(m_vol);
            SetMusicVolume(m_vol);
            var sfx = PlayerPrefs.GetFloat(sfxVolume, 1);
            sfxSlider.SetValueWithoutNotify(sfx);
            SetSFXVolume(sfx);
        }

        public void SetMusicVolume(float volume)
        {
            audioMixer.SetFloat(musicVolume, volume);
            PlayerPrefs.SetFloat(musicVolume, volume);
        }

        public void SetMasterVolume(float volume)
        {
            audioMixer.SetFloat(masterVolume, volume);
            PlayerPrefs.SetFloat(masterVolume, volume);
        }

        public void SetSFXVolume(float volume)
        {
            audioMixer.SetFloat(sfxVolume, volume);
            PlayerPrefs.SetFloat(sfxVolume, volume);
        }
    }
}