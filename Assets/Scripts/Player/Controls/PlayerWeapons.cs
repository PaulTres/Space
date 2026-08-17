using UnityEngine;
using UnityEngine.InputSystem;
namespace Player
{
    public class PlayerWeapons : MonoBehaviour, PlayerControls.IWeaponsActions
    {
        private void OnEnable()
        {
            GameManager.InputActions.Weapons.SetCallbacks(this);
        }

        public void OnNewaction(InputAction.CallbackContext context)
        {
            if (context.canceled) Debug.Log("Hold");
            else if (context.performed) Debug.Log("Tap");
        }
    }
}
