using Timers;
using TMPro;
using UnityEngine;
namespace Player
{

    public class Widget : MonoBehaviour
    {
        protected RectTransform rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
                return rectTransform;
            }
        }
        protected TextMeshProUGUI text;
        public TextMeshProUGUI Text
        {
            get
            {
                if (text == null) text = GetComponentInChildren<TextMeshProUGUI>();
                return text;
            }
        }
        public float LifeTime { get; set; }
        public event System.Action<Widget> OnDisableEvent = delegate { };
        protected CountdownTimer lifetimeTimer = new(1);

        protected void Awake()
        {
            lifetimeTimer.OnTimerStop += () => gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            lifetimeTimer.Reset(LifeTime);
            lifetimeTimer.Start();
        }

        protected virtual void OnDisable()
        {
            if (lifetimeTimer.IsRunning) lifetimeTimer.Stop();
        }

        protected virtual void OnDestroy()
        {
            lifetimeTimer?.Dispose();
            OnDisableEvent = null;
        }
    }
}
