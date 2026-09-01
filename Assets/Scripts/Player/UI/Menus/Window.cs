using UnityEngine;
namespace Player
{

    [RequireComponent(typeof(CanvasGroup))]
    public class Window : MonoBehaviour
    {
        protected MenuPDA menuPDA;
        protected CanvasGroup canvasGroup;

        public MenuPDA MenuPDA
        {
            get
            {
                if (menuPDA == null) menuPDA = GetComponentInParent<MenuPDA>();
                return menuPDA;
            }
        }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
                return canvasGroup;
            }
        }

        public bool Interactable
        {
            get => CanvasGroup.interactable;
            set => CanvasGroup.interactable = value;
        }

        public void Open()
        {
            MenuPDA.AddOnTop(this);
        }

        public void Close()
        {
            MenuPDA.CloseWindow(this);
        }

        public void Activate()
        {
            CanvasGroup.interactable = true;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Sets textParent group to be non-interactable and disables the game object.
        /// </summary>
        public void Deactivate()
        {
            CanvasGroup.interactable = false;
            gameObject.SetActive(false);
        }
    }
}
