using UnityEngine;
using UnityEngine.UI;

namespace Utilities
{
    public class ButtonSoundHook : MonoBehaviour
    {
        protected void Awake()
        {
            if (TryGetComponent<Button>(out var b)) b.onClick.AddListener(PlayClickSound);
        }

        public void PlayClickSound()
        {
            if (GameManager.Player.Active()) GlobalConfig.ClickSound.Play(GameManager.Player.Position);
            else GlobalConfig.ClickSound.Play(Vector3.zero);
        }
    }
}