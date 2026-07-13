using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
namespace Player
{
    [CreateAssetMenu(fileName = "UIInputReader", menuName = "ScriptableObjects/UIInputReader")]
    public class UIInputReader : ScriptableObject, PlayerControls.IUIActions
    {
        public event UnityAction OnEscapeEvent = delegate { };
        public PlayerControls inputActions;
        public void EnablePlayerActions()
        {
            inputActions ??= new PlayerControls();
            inputActions.UI.SetCallbacks(this);
            inputActions.Enable();
            OnEscapeEvent ??= delegate { };
        }

        public void DisablePlayerActions()
        {
            if (inputActions != null)
            {
                inputActions.BasicControls.SetCallbacks(null);
                inputActions.Disable();
                OnEscapeEvent = null;
            }
        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                //Debug.Log(System.DateTime.Now + " escape");
                OnEscapeEvent?.Invoke();
            }
        }
    }
}
