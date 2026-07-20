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
    public static void ResetProgress()
    {
        currentSave = new();
    }

    public static void NewGame()
    {
        currentSave = new();
        LoadLobby();
    }

    public static void Load(string fileName)
    {
        var s = GameSave.Load(fileName);
        currentSave = s ?? new();
        LoadLobby();
    }

    public static void Save()
    {
        currentSave.Save();
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
        MissionTime = Time.time - Mission.CurrentMission.StartTime;
        SceneManager.LoadScene(GlobalConfig.EndMissionSceneIndex);
    }
    #endregion
}
