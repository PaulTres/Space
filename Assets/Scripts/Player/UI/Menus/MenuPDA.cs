using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Player
{
    public class MenuPDA : MonoBehaviour, PlayerControls.IUIActions
    {
        protected Stack<Window> windows = new();
        [SerializeField] protected Window[] initialWindows;
        [SerializeField] protected SceneFadeInAndOut sceneFadeScript;
        public Window Top => windows.Peek();

        protected virtual void OnEnable()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            GameManager.InputActions.UI.AddCallbacks(this);
            if (initialWindows != null)
            {
                for (int i = 0; i < initialWindows.Length; i++)
                {
                    AddOnTop(initialWindows[i]);
                }
            }
        }

        protected virtual void OnDisable()
        {
            while (windows.Count > 0) Pop(true);
            GameManager.InputActions.UI.RemoveCallbacks(this);
        }

        public void AddOnTop(Window window, bool deactivatePrev = true)
        {
            if (window == null)
            {
                Debug.LogWarning($"Cannot add empty textParent group on stack for {transform}!");
                return;
            }
            if (deactivatePrev && windows.Count > 0)
            {
                var top = windows.Peek();
                top.Deactivate();
            }
            windows.Push(window);
            //Debugging.Log(System.DateTime.Now + " opening " + window.gameObject);
            window.Activate();
        }

        public void CloseWindow(Window window)
        {
            while (windows.Count > 0)
            {
                var p = windows.Pop();
                p.Deactivate();
                //Debugging.Log(System.DateTime.Now + " deactivated " + p.transform);
                if (p == window)
                {
                    if (windows.Count > 0)
                    {
                        windows.Peek().Activate();
                    }
                    break;
                }
            }
        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            if (context.started && gameObject.activeSelf) Pop();
        }

        public virtual void Pop() => Pop(false);

        public Window Pop(bool allowEmpty, bool activatePrevious = true)
        {
            if (windows.Count == 0) return null;
            if (!allowEmpty && windows.Count == 1) return null;
            var top = windows.Pop();
            top.Deactivate();
            //Debugging.Log(System.DateTime.Now + " deactivated " + top.transform);
            if (activatePrevious && windows.Count > 0)
            {
                windows.Peek().Activate();
            }
            return top;
        }
    }
}
