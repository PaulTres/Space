using EventBus;
using UnityEngine;
namespace Player
{
    public class PauseScreen : MenuPDA
    {
        /// <summary>
        /// Used to send a global event when the game is paused/unpaused.
        /// </summary>
        public struct PauseEvent : IEvent
        {
            /// <summary>
            /// If true, the game is paused right now, and most inputs should be disabled.
            /// </summary>
            public bool Paused;

            public PauseEvent(bool paused) => Paused = paused;
        }

        [SerializeField] protected bool stopTime = true;
        [SerializeField] protected bool sendEvent = true;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (sendEvent) EventBus<PauseEvent>.Raise(new(true));
            if (stopTime) Time.timeScale = 0;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (sendEvent) EventBus<PauseEvent>.Raise(new(false));
            Time.timeScale = 1;
        }

        public override void Pop()
        {
            if (windows.Count == 1) gameObject.SetActive(false);
            else base.Pop();
        }

        public void ReturnToMenu() => GameManager.LoadMainMenu();
    }
}