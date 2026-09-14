using Level;
using Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utilities;

public static class GameManager
{
    static PlayerControls inputActions;
    public const string RebindPrefsKey = "InputRebinds";
    /// <summary>
    /// PlayerControls global instance.
    /// </summary>
    public static PlayerControls InputActions
    {
        get
        {
            if (inputActions == null)
            {
                inputActions = new();
                if (PlayerPrefs.HasKey(RebindPrefsKey))
                {
                    string json = PlayerPrefs.GetString(RebindPrefsKey);
                    inputActions.LoadBindingOverridesFromJson(json);
                }
                inputActions.Enable();
            }
            return inputActions;
        }
    }
    static GameSave currentSave;
    public static float MissionTime { get; set; } = 0;
    /// <summary>
    /// All the units that have been destroyed over the course of a mission.
    /// </summary>
    public static List<UnitDestroyed> UnitsDestroyed { get; } = new();
    public static PlayerShip Player { get; set; } = null;
    /// <summary>
    /// The latest mission the player can play.
    /// </summary>
    public static int LatestMissionIndex
    {
        get
        {
            if (currentSave == null) return 0;
            return Mathf.Min(currentSave.CurrentMissionIndex, Mission.Campaign.Count);
        }
    }
    public static int Difficulty
    {
        get
        {
            if (currentSave == null)
            {
                //Debug.LogWarning("No save file found!");
                return 1;
            }
            return currentSave.Difficulty;
        }
    }
    public static bool CampaignCompleted => LatestMissionIndex >= Mission.Campaign.Count;
    /// <summary>
    /// The mission the player has chosen to play.
    /// </summary>
    public static int ChosenMissionIndex { get; set; }

    #region Progress
    public static void NewGame(int difficulty)
    {
        currentSave = new(0, difficulty);
        currentSave.Save();
        LoadMissionSelection();
    }

    public static void LoadSave(string fileName)
    {
        var s = GameSave.Load(fileName);
        if (s == null) return;
        currentSave = s;
        LoadMissionSelection();
    }

    public static void OnMissionCompleted()
    {
        if (ChosenMissionIndex == LatestMissionIndex)
        {
            currentSave.CurrentMissionIndex++;
            currentSave.Save();
        }
    }
    #endregion

    #region Scene loading
    public static void LoadTutorial()
    {
        SceneManager.LoadScene(GlobalConfig.TutorialSceneIndex);
    }

    public static void LoadMainMenu()
    {
        SceneManager.LoadScene(GlobalConfig.MainMenuSceneIndex);
    }

    public static void LoadMissionSelection()
    {
        SceneManager.LoadScene(GlobalConfig.MissionSceneIndex);
    }

    public static void LoadMission()
    {
        LoadMission(ChosenMissionIndex);
    }

    /// <summary>
    /// Will load the scene with name "Mission_{index+1}".
    /// </summary>
    /// <param name="index"></param>
    public static void LoadMission(int index)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene($"Mission_{index + 1}");
    }

    public static void EndMission()
    {
        if (!Player.Active()) return;
        UnitManager.Clear();
        OnMissionCompleted();
        MissionTime = Time.time - MissionManager.StartTime;
        SceneManager.LoadScene(GlobalConfig.EndMissionSceneIndex);
    }
    #endregion
}
