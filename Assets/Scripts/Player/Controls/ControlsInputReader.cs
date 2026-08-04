using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
namespace Player
{

    [CreateAssetMenu(fileName = "ControlsInputReader", menuName = "ScriptableObjects/ControlsInputReader")]
    public class ControlsInputReader : ScriptableObject, PlayerControls.IBasicControlsActions
    {
        public PlayerControls inputActions;
        public Vector2 LookDelta => inputActions.BasicControls.MouseLook.ReadValue<Vector2>();
        public event UnityAction<Vector3> MoveEvent = delegate { };
        public event UnityAction<float> RollEvent = delegate { };
        public event UnityAction<float> ZoomEvent = delegate { };
        public event UnityAction<float> FreeLookToggleEvent = delegate { };
        public event UnityAction ResetEvent = delegate { };
        public event UnityAction DampenerToggleEvent = delegate { };
        public event UnityAction FindTargetEvent = delegate { };

        public void EnablePlayerActions()
        {
            inputActions ??= new PlayerControls();
            inputActions.BasicControls.SetCallbacks(this);
            inputActions.Enable();
            MoveEvent ??= delegate { };
            RollEvent ??= delegate { };
            FreeLookToggleEvent ??= delegate { };
            ZoomEvent ??= delegate { };
            ResetEvent ??= delegate { };
            DampenerToggleEvent ??= delegate { };
            FindTargetEvent ??= delegate { };
        }

        public void DisablePlayerActions()
        {
            if (inputActions != null)
            {
                inputActions.BasicControls.SetCallbacks(null);
                inputActions.Disable();
                DisableCallbacks();
            }
        }

        public void DisableCallbacks()
        {
            MoveEvent = delegate { };
            ResetEvent = delegate { };
            RollEvent = ZoomEvent = delegate { };
            FreeLookToggleEvent = delegate { };
            DampenerToggleEvent = delegate { };
            FindTargetEvent = delegate { };
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector3>());
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            RollEvent?.Invoke(context.ReadValue<float>());
        }

        /// <summary>
        /// This method only exists to satisfy the interface implementation. The actual mouse
        /// input is read directly from the LookDelta property.
        /// </summary>
        /// <param name="context"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void OnMouseLook(InputAction.CallbackContext context)
        {
            //nothing
        }

        public void OnFreeLookToggle(InputAction.CallbackContext context)
        {
            FreeLookToggleEvent?.Invoke(context.ReadValue<float>());
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            ZoomEvent?.Invoke(context.ReadValue<float>());
        }

        public void OnReset(InputAction.CallbackContext context)
        {
            if (context.started) ResetEvent?.Invoke();
        }

        public void OnToggleDampeners(InputAction.CallbackContext context)
        {
            if (context.started) DampenerToggleEvent?.Invoke();
        }

        public void OnFindTarget(InputAction.CallbackContext context)
        {
            if (context.started) FindTargetEvent?.Invoke();
        }
    }
}
