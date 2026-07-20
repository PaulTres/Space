using Timers;
using UnityEngine;
using Utilities;

public class MissionManager : Singleton<MissionManager>
{
    protected CountdownTimer endMissionTimer;
    [Tooltip("Set to null for normal behaviour. If assigned, the set mission will " +
        "be player no matter what.")]
    [SerializeField] protected Mission mission;

    protected override void InitializeSingleton()
    {
        base.InitializeSingleton();
        if (mission != null) Mission.CurrentMission = mission;
        if (Mission.CurrentMission.TimeLimit > 0)
        {
            endMissionTimer = new(GlobalConfig.MissionEndDuration);
            endMissionTimer.OnTimerStop += () => GameManager.EndMission();
        }
        Mission.CurrentMission.Initialize();
    }

    protected override void ClearSingleton()
    {
        base.ClearSingleton();
        endMissionTimer?.Dispose();
    }

    public void EndMission()
    {
        endMissionTimer?.Start();
    }
}
