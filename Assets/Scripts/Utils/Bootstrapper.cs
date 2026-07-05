using Effects;
using Player;
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
        protected void Start()
        {
            PoolableEffect.InitializePool();
            Bullet.InitializePool();
            SceneManager.activeSceneChanged += (s1, s2) =>
            {
                EventBus.EventBusUtil.ClearAllBuses();
            };

            VideoSettingsMenu.LoadVideoSettings();
            AudioSettingsMenu.LoadAudioSettings(audioMixer);
            SceneManager.LoadScene(1);
        }
    }
}