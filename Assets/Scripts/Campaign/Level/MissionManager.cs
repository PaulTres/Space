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
        endMissionTimer = new(GlobalConfig.MissionEndDuration);
        endMissionTimer.OnTimerStop += () => GameManager.EndMission();

    }

    protected void Start()
    {
        Mission.CurrentMission.Initialize();
    }

    protected override void ClearSingleton()
    {
        base.ClearSingleton();
        endMissionTimer?.Dispose();
    }

    public void EndMission()
    {
        //.Log("Ending mission");
        endMissionTimer?.Start();
    }

    public void StopEndTimer()
    {
        endMissionTimer?.Stop();
        endMissionTimer?.Reset();
    }
}
