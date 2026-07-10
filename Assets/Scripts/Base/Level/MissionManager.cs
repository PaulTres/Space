using Timers;
using Utilities;

public class MissionManager : Singleton<MissionManager>
{
    protected CountdownTimer endMissionTimer;

    protected override void InitializeSingleton()
    {
        base.InitializeSingleton();
        endMissionTimer = new(GlobalConfig.MissionEndDuration);
        endMissionTimer.OnTimerStop += () => GameManager.EndMission();
        Mission.CurrentMission.Initialize();
    }

    protected override void ClearSingleton()
    {
        base.ClearSingleton();
        endMissionTimer?.Dispose();
    }

    public void EndMission()
    {
        endMissionTimer.Start();
    }
}
