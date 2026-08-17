using EventBus;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace Player
{
    public class UISystem : MonoBehaviour, PlayerControls.IUIActions
    {
        [SerializeField] protected RectTransform textParent;
        [SerializeField] protected Widget widgetPrefab;
        protected GameObject pauseMenu;
        protected readonly Stack<Widget> textWidgetPool = new();
        protected Widget currentWidget;

        #region Setup
        protected virtual void Awake()
        {
            pauseMenu = GetComponentInChildren<PauseScreen>(true).gameObject;
            EventBus<ShowText>.AddActions(SpawnText);
            GameManager.InputActions.UI.SetCallbacks(this);
        }

        protected virtual void OnDisable()
        {
            GameManager.InputActions.UI.RemoveCallbacks(this);
            currentWidget = null;
        }

        protected virtual void OnDestroy()
        {

            textWidgetPool.Clear();
            EventBus<ShowText>.RemoveActions(SpawnText);
        }
        #endregion

        public void OnEscape(InputAction.CallbackContext context)
        {
            if (context.started) pauseMenu.SetActive(true);
        }

        #region Widgets 
        public static void ShowText(ShowText @event)
        {
            EventBus<ShowText>.Raise(@event);
        }

        public void SpawnText(ShowText @event)
        {
            if (currentWidget != null && currentWidget.gameObject.activeSelf)
            {
                //we already have an acive widget, we need to disable it
                currentWidget.gameObject.SetActive(false);
            }
            var w = GetWidget();
            w.LifeTime = @event.LifeTime;
            w.Text.text = @event.Text;
            w.Text.color = @event.TextColor;
            w.RectTransform.anchorMax = @event.AnchorMax;
            w.RectTransform.anchorMin = @event.AnchorMin;
            w.RectTransform.anchoredPosition = @event.AnchoredPosition;
            w.gameObject.SetActive(true);
            currentWidget = w;
        }

        protected void ReleaseWidget(Widget widget)
        {
            if (widget == null) return;
            widget.gameObject.SetActive(false);
            textWidgetPool.Push(widget);
        }

        protected Widget GetWidget()
        {
            if (textWidgetPool.Count == 0)
            {
                var w = Instantiate(widgetPrefab, textParent.transform);
                w.OnDisableEvent += ReleaseWidget;
                return w;
            }
            return textWidgetPool.Pop();
        }
        #endregion
    }
}
