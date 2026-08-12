using Player;
using System.Collections.Generic;
using Timers;
using UnityEngine;
using UnityEngine.Pool;
using Utilities;

namespace Level
{
    public class MissionManager : Singleton<MissionManager>
    {
        protected ObjectPool<CountdownTimer> timerPool;
        protected HashSet<CountdownTimer> activeTimers = new();
        [Tooltip("Set to null for normal behaviour. If assigned, the set mission will " +
            "be player no matter what.")]
        [SerializeField] protected Mission mission;

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            timerPool = new(() => new CountdownTimer(1),
                (t) => t.OnTimerStop = () => OnTimerStop(t),
                null, (t) => t?.Dispose());
            if (mission != null) Mission.CurrentMission = mission;
        }

        protected void Start()
        {
            Mission.CurrentMission.Initialize();
        }

        protected override void ClearSingleton()
        {
            base.ClearSingleton();
            timerPool.Clear();
        }

        protected void OnTimerStop(CountdownTimer t, bool removeFromSet = true)
        {
            timerPool.Release(t);
            if (removeFromSet) activeTimers.Remove(t);
            t.OnTimerStop = null;
        }

        public void StopAllTimers()
        {
            foreach (var timer in activeTimers)
            {
                timer.OnTimerStop = null;
                timer.Stop();
                OnTimerStop(timer, false);
            }
            activeTimers.Clear();
        }

        /// <summary>
        /// Executes a callback after a delay.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        public void ExecuteAction(System.Action action, float delay)
        {
            if (delay == 0) action();
            else
            {
                var timer = timerPool.Get();
                timer.Reset(delay);
                timer.OnTimerStop += action;
                timer.Start();
            }
        }

        public void MissionVictory(string text = "Victory!")
        {
            if (GameManager.Player != null)
            {
                GameManager.Player.CannotDie = true;
                UISystem.ShowText(new(Vector3.zero, GlobalConfig.MissionEndDuration, text, Color.green));
                ExecuteAction(() => GameManager.EndMission(), GlobalConfig.MissionEndDuration);
            }
        }

        public void MissionFailed(string text = "Defeat")
        {
            if (GameManager.Player != null)
            {
                GameManager.Player.CannotDie = true;
                UISystem.ShowText(new(Vector3.zero, GlobalConfig.MissionEndDuration, text, Color.red));
                ExecuteAction(() => GameManager.Player.gameObject.SetActive(false), GlobalConfig.MissionEndDuration);
            }
        }
    }
}