using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace Player
{
    public class EndCredits : MonoBehaviour, PlayerControls.IUIActions
    {
        [SerializeField] protected SceneFadeInAndOut sceneFadeScript;

        protected void OnEnable()
        {
            GlobalConfig.ClickSound.Play(transform.position, requiresPlayer: false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            GameManager.InputActions.UI.AddCallbacks(this);
        }

        protected void OnDisable()
        {
            GameManager.InputActions.UI.RemoveCallbacks(this);
        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            if (!context.started || !gameObject.activeSelf) return;
            if (!sceneFadeScript.IsFading)
            {
                GlobalConfig.ClickSound.Play(transform.position, requiresPlayer: false);
                sceneFadeScript.FadeOut(() => GameManager.LoadMainMenu());
            }
        }
    }
}