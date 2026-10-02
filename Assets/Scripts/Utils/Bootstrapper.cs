using Effects;
using Level;
using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using Weapons;

namespace Utilities
{
    public class Bootstrapper : MonoBehaviour
    {
        protected async void Start()
        {
            PoolableEffect.InitializePool();
            Bullet.InitializePool();
            SceneManager.activeSceneChanged += (s1, s2) =>
            {
                Time.timeScale = 1;
                //UnitManager.Clear();
                //EventBus.EventBusUtil.ClearAllBuses();
                AudioSettingsMenu.LoadSettings(GlobalConfig.AudioMixer);
            };
            VideoSettingsMenu.LoadVideoSettings();
            ControlsSettingsMenu.LoadSettings();

            await GlobalConfig.LoadInstance();
            await UnitData.LoadAllSpawnableUnits();
            await Mission.LoadCampaignMissions();

            SceneManager.LoadScene(GlobalConfig.MainMenuSceneIndex);
        }
    }
}