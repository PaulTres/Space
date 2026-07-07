using Effects;
using Player;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Weapons;
namespace Utilities
{
    public class Bootstrapper : MonoBehaviour
    {
        [Tooltip("The audio mixer to use for audio settings.")]
        [SerializeField] protected AudioMixer audioMixer;

        protected async void Start()
        {
            PoolableEffect.InitializePool();
            Bullet.InitializePool();
            SceneManager.activeSceneChanged += (s1, s2) =>
            {
                EventBus.EventBusUtil.ClearAllBuses();
            };
            VideoSettingsMenu.LoadVideoSettings();
            AudioSettingsMenu.LoadAudioSettings(audioMixer);

            await Task.WhenAll(GlobalConfig.LoadInstance(), UnitData.LoadAllSpawnableUnits());

            SceneManager.LoadScene(GlobalConfig.MainMenuSceneIndex);
        }
    }
}