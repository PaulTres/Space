using EventBus;
using System.Collections.Generic;
using UnityEngine;
namespace Player
{
    public class UISystem : MonoBehaviour
    {
        [SerializeField] protected UIInputReader inputReader;
        [SerializeField] protected RectTransform textParent;
        [SerializeField] protected Widget widgetPrefab;
        [SerializeField] protected GameObject pauseMenu;
        protected readonly Stack<Widget> textWidgetPool = new();

        #region Setup
        protected virtual void Awake()
        {
            EventBus<ShowText>.AddActions(SpawnText);
            inputReader.OnEscapeEvent += OpenPauseMenu;
        }

        protected virtual void OnDestroy()
        {
            inputReader.OnEscapeEvent -= OpenPauseMenu;
            textWidgetPool.Clear();
            EventBus<ShowText>.RemoveActions(SpawnText);
        }
        #endregion

        public void OpenPauseMenu()
        {
            pauseMenu.SetActive(true);
        }

        #region Widgets 
        public static void ShowText(ShowText @event)
        {
            EventBus<ShowText>.Raise(@event);
        }

        public void SpawnText(ShowText @event)
        {
            var w = GetWidget();
            w.LifeTime = @event.LifeTime;
            w.Text.text = @event.Text;
            w.Text.color = @event.TextColor;
            w.RectTransform.anchorMax = @event.AnchorMax;
            w.RectTransform.anchorMin = @event.AnchorMin;
            w.RectTransform.anchoredPosition = @event.AnchoredPosition;
            w.gameObject.SetActive(true);
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
