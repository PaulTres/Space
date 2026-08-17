using HP;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace Player
{
    public class PlayerGameUI : MonoBehaviour, PlayerControls.IUIActions
    {
        [SerializeField] protected GameObject pauseMenu;
        [SerializeField] protected Image hullImage;
        protected HPComponent hpComponent;

        protected void Awake()
        {
            hpComponent = GetComponentInParent<HPComponent>();
            hpComponent.OnDamageTaken += UpdateHullImage;
        }

        protected void OnEnable()
        {
            GameManager.InputActions.UI.SetCallbacks(this);
            hullImage.fillAmount = 1;
        }

        protected void OnDisable()
        {
            GameManager.InputActions.UI.RemoveCallbacks(this);
        }

        protected void UpdateHullImage()
        {
            hullImage.fillAmount = hpComponent.CurrentHP / hpComponent.MaxHP;
        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            if (context.started) pauseMenu.SetActive(true);
        }
    }
}
