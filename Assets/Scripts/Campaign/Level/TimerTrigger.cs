using Timers;
using UnityEngine;

namespace Level
{
    public class TimerTrigger : Trigger
    {
        [SerializeField] protected float time;
        protected CountdownTimer timer;
        [SerializeField] protected bool fireOnEnable = true;

        protected void OnEnable()
        {
            if (timer == null)
            {
                timer = new(time);
                timer.OnTimerStop += () => Delegate?.Invoke();
            }
            if (fireOnEnable) StartTimer();
        }

        public void StartTimer()
        {
            if (!timer.IsRunning) timer.Start();
        }

        protected void OnDestroy()
        {
            timer?.Dispose();
        }
    }
}