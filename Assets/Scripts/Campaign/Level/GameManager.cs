using Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameManager
{
    static GameSave currentSave;
    public static float MissionTime { get; set; } = 0;
    public static List<ObjectDestroyed> UnitsDestroyed { get; } = new();
    public static Unit Player { get; set; } = null;
    public static int LatestMissionIndex => currentSave != null ? currentSave.CurrentMissionIndex : 0;
    public static int ChosenMissionIndex { get; set; }

    #region Pausing
    public static bool IsPaused => Time.timeScale == 0f;

    public static void Pause()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0;
    }

    public static void TogglePause()
    {
        Cursor.visible = !Cursor.visible;
        Cursor.lockState = IsPaused ? CursorLockMode.Locked : CursorLockMode.None;
        Time.timeScale = IsPaused ? 1f : 0f;
    }

    public static void ResumeGame()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    #endregion

    #region Progress
    public static void NewGame()
    {
        currentSave = new(0);
        LoadLobby();
    }

    public static void Load(string fileName)
    {
        var s = GameSave.Load(fileName);
        currentSave = s ?? new(0);
        LoadLobby();
    }

    public static void OnMissionCompleted()
    {
        if (ChosenMissionIndex == LatestMissionIndex)
        {
            currentSave.CurrentMissionIndex = Mathf.Min(Mission.Campaign.Count, ChosenMissionIndex + 1);
            currentSave.Save();
        }
    }
    #endregion

    #region Scene loading
    public static void LoadMainMenu()
    {
        SceneManager.LoadScene(GlobalConfig.MainMenuSceneIndex);
    }

    public static void LoadLobby()
    {
        UnitsDestroyed.Clear();
        SceneManager.LoadScene(GlobalConfig.LobbySceneIndex);
    }

    public static void LoadMission()
    {
        SceneManager.LoadScene(GlobalConfig.MissionSceneIndex);
    }

    public static void EndMission()
    {
        OnMissionCompleted();
        MissionTime = Time.time - Mission.CurrentMission.StartTime;
        SceneManager.LoadScene(GlobalConfig.EndMissionSceneIndex);
    }
    #endregion
}
