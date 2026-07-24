using EventBus;
using System.Collections.Generic;
using UnityEngine;
namespace Player
{

    public class UISystem : MonoBehaviour
    {
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected Widget widgetPrefab;
        protected readonly Stack<Widget> textWidgetPool = new();

        public static void ShowText(ShowText @event)
        {
            EventBus<ShowText>.Raise(@event);
        }

        protected virtual void Awake()
        {
            EventBus<ShowText>.AddActions(SpawnText);
            if (canvas == null) canvas = GetComponentInParent<Canvas>();
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
                var w = Instantiate(widgetPrefab, canvas.transform);
                w.OnDisableEvent += ReleaseWidget;
                return w;
            }
            return textWidgetPool.Pop();
        }

        protected virtual void OnDestroy()
        {
            textWidgetPool.Clear();
            EventBus<ShowText>.RemoveActions(SpawnText);
        }
    }
}
