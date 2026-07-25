using Effects;
using Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Weapons;
namespace Utilities
{
    public class Bootstrapper : MonoBehaviour
    {
        [MenuItem("Tools/Start game")]
        public static void LoadBootstrapperScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/BootstrapperScene.unity");
            EditorApplication.isPlaying = true;
        }

        [Tooltip("The audio mixer to use for audio settings.")]
        [SerializeField] protected AudioMixer audioMixer;

        protected async void Start()
        {
            PoolableEffect.InitializePool();
            Bullet.InitializePool();
            SceneManager.activeSceneChanged += (s1, s2) =>
            {
                Time.timeScale = 1;
                //EventBus.EventBusUtil.ClearAllBuses();
            };
            VideoSettingsMenu.LoadVideoSettings();
            AudioSettingsMenu.LoadAudioSettings(audioMixer);

            await GlobalConfig.LoadInstance();
            await UnitData.LoadAllSpawnableUnits();
            await Mission.LoadCampaignMissions();

            SceneManager.LoadScene(GlobalConfig.MainMenuSceneIndex);
        }
    }
}