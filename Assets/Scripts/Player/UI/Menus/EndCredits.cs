using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;

namespace Player
{
    public class EndCredits : MonoBehaviour, PlayerControls.IUIActions
    {
        [SerializeField] protected float fadeInDuration = 1, fadeOutDuration = 1;
        [SerializeField] protected Image blackImage;
        protected bool fading = false;

        protected void OnEnable()
        {
            GlobalConfig.ClickSound.Play(transform.position, requiresPlayer: false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameManager.InputActions.UI.AddCallbacks(this);
            blackImage.enabled = true;
            blackImage.CrossFadeAlpha(1, 0, true);
            blackImage.CrossFadeAlpha(0, fadeInDuration, true);
        }

        protected void OnDisable()
        {
            GameManager.InputActions.UI.RemoveCallbacks(this);
        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            if (!context.started || !gameObject.activeSelf) return;
            if (blackImage.canvasRenderer.GetAlpha() < 0.01f && !fading)
            {
                GlobalConfig.ClickSound.Play(transform.position, requiresPlayer: false);
                fading = true;
                blackImage.CrossFadeAlpha(1, fadeOutDuration, true);
            }
        }

        private void Update()
        {
            if (fading && blackImage.canvasRenderer.GetAlpha() > .99f) GameManager.LoadMainMenu();
        }
    }
}