using Effects;
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
        protected ObjectPool<CountdownTimer> timerPoolProperty
        {
            get
            {
                timerPool ??= new(() => new CountdownTimer(1),
                 (t) => t.OnTimerStop = () => OnTimerStop(t),
                 null, (t) => t?.Dispose());
                return timerPool;
            }
        }
        protected HashSet<CountdownTimer> activeTimers = new();
        public static float StartTime { get; protected set; }
        /// <summary>
        /// If true, victory/failure can no longer occur.
        /// </summary>
        public bool LockMissionState { get; protected set; }

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            PoolableEffect.ClearPool();
            LockMissionState = false;
            Time.timeScale = 1;
            StartTime = Time.time;
            GameManager.UnitsDestroyed.Clear();
        }

        protected void OnEnable() => LockMissionState = false;

        protected override void ClearSingleton()
        {
            base.ClearSingleton();
            StopAllTimers();
            timerPoolProperty.Clear();
            PoolableEffect.ClearPool();
        }

        protected void OnTimerStop(CountdownTimer t, bool removeFromSet = true)
        {
            timerPoolProperty.Release(t);
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
        public void ExecuteAction(System.Action action, float delay = 0)
        {
            if (delay == 0)
            {
                action();
            }
            else
            {
                var timer = timerPoolProperty.Get();
                timer.Reset(delay);
                timer.OnTimerStop += action;
                timer.Start();
                activeTimers.Add(timer);
            }
        }

        public void MissionVictory(string text = "Victory!")
        {
            if (LockMissionState) Debug.Log("Mission state locked.");
            if (!LockMissionState && GameManager.Player.Active())
            {
                LockMissionState = true;
                GameManager.Player.CannotDie = true;
                UISystem.ShowText(new(Vector3.zero, GlobalConfig.MissionSuccessEndDuration, text, Color.green));
                ExecuteAction(() => GameManager.EndMission(), GlobalConfig.MissionSuccessEndDuration);
            }
        }

        /// <summary>
        /// This method is called when the mission fails. It shows a failure 
        /// message and disables the player ship after a delay. Do not use this 
        /// for player death, as it will prevent the player from dying and 
        /// disable the ship instead. Use PlayerShip.Die() for player death.
        /// </summary>
        /// <param name="text"></param>
        public void MissionFailed(string text = "Defeat")
        {
            if (LockMissionState) Debug.Log("Mission state locked.");
            if (!LockMissionState && GameManager.Player.Active())
            {
                //Debug.Log("Defeat");
                LockMissionState = true;
                GameManager.Player.CannotDie = true;
                UISystem.ShowText(new(Vector3.zero, GlobalConfig.MissionFailureEndDuration, text, Color.red));
                ExecuteAction(() => GameManager.Player.MissionFailed(), GlobalConfig.MissionFailureEndDuration);
            }
            //else Debug.Log("Player: " + GameManager.Player.Active());
        }
    }
}