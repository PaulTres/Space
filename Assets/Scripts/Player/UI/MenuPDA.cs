using System.Collections.Generic;
using UnityEngine;
namespace Player
{
    public class MenuPDA : MonoBehaviour
    {
        [SerializeField] protected UIInputReader inputReader;
        protected Stack<Window> windows = new();
        [SerializeField] protected Window[] initialWindows;
        public Window Top => windows.Peek();
        protected virtual void OnEnable()
        {
            inputReader.EnablePlayerActions();
            inputReader.OnEscapeEvent += Pop;
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
            inputReader.OnEscapeEvent -= Pop;
            //inputReader.DisablePlayerActions();
            while (windows.Count > 0) Pop(true);
        }
        public void AddOnTop(Window window, bool deactivatePrev = true)
        {
            if (window == null)
            {
                Debug.LogWarning($"Cannot add empty canvas group on stack for {transform}!");
                return;
            }
            if (deactivatePrev && windows.Count > 0)
            {
                var top = windows.Peek();
                top.Deactivate();
            }
            windows.Push(window);
            //Debug.Log(System.DateTime.Now + " opening " + window.gameObject);
            window.Activate();
        }
        public void CloseWindow(Window window)
        {
            while (windows.Count > 0)
            {
                var p = windows.Pop();
                p.Deactivate();
                //Debug.Log(System.DateTime.Now + " deactivated " + p.transform);
                if (p == window) break;
            }
        }
        public virtual void Pop()
        {
            Pop(false);
        }
        public Window Pop(bool allowEmpty, bool activatePrevious = true)
        {
            if (windows.Count == 0) return null;
            if (!allowEmpty && windows.Count == 1) return null;
            var top = windows.Pop();
            top.Deactivate();
            //Debug.Log(System.DateTime.Now + " deactivated " + top.transform);
            if (activatePrevious && windows.Count > 0)
            {
                windows.Peek().Activate();
            }
            return top;
        }
    }
}
