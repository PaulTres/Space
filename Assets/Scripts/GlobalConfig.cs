using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class has a bunch of global parameters and shit.
/// </summary>
public static class GlobalConfig
{
    #region Units
    public static readonly List<UnitData> PossibleAllyUnits = new();
    public static readonly List<UnitData> PossibleEnemyUnits = new();

    public static UnitData GetPlayerVariant(UnitData unitData)
    {
        throw new System.NotImplementedException();
    }
    #endregion

    public static string SaveFileLocation => Application.persistentDataPath;
    /// <summary>
    /// This is the size that the buffer is initialized with when checking for missiles.
    /// Might be worth expanding to 100.
    /// </summary>
    public static readonly int MaxMissileCheckBufferSize = 50;
    /// <summary>
    /// How many seconds to wait between ending the mission and loading the end mission screen.
    /// </summary>
    public static readonly float MissionEndDuration = 3;
    public static readonly float ParticleSystemLifetimeCheckInterval = 1;
    public static readonly int MissionSceneIndex = 3, EndMissionSceneIndex = 4, LobbySceneIndex = 2;

    #region Layers and layer masks
    static readonly Dictionary<int, LayerMask> bulletCollisionMasks = new()
    {
        {7, 1 << 0 | 1 << 8}, {9,1<<0|1<<6},{10, 1<<0|1<<6|1<<8}
    };
    static readonly Dictionary<int, int> bulletLayerCorrespondence = new()
    {
        { 6, 7 }, { 8, 9 },{0,10}
    };
    static readonly Dictionary<int, LayerMask> bulletCheckMask = new()
    {
        {6, 1 << 9 | 1 << 10}, {8,1<<7|1<<10},{0, 1<<10|1<<7|1<<9}
    };

    /// <summary>
    /// Returns the layer mask that an object should use to check for enemy bullets.
    /// </summary>
    /// <param name="checkerLayer"></param>
    /// <returns></returns>
    public static LayerMask GetEnemyBulletCheckMask(int checkerLayer)
    {
        return bulletCheckMask[checkerLayer];
    }

    /// <summary>
    /// Returns the collision mask that a bullet should use, based on its own layer.
    /// </summary>
    /// <param name="bulletLayer"></param>
    /// <returns></returns>
    public static LayerMask GetBulletCollisionMask(int bulletLayer)
    {
        return bulletCollisionMasks[bulletLayer];
    }

    /// <summary>
    /// Returns a layer that a bullet should use, given the layer of its owner.
    /// </summary>
    /// <param name="ownerLayer"></param>
    /// <returns></returns>
    public static int GetBulletLayer(int ownerLayer)
    {
        return bulletLayerCorrespondence[ownerLayer];
    }
    #endregion

    #region Colors
    static readonly Dictionary<int, Color> teamColors = new()
    {
        { 0,Color.white},{6,Color.blue},{8,Color.red},
        { 10,Color.white},{7,Color.blue},{9,Color.red}
    };

    /// <summary>
    /// Returns what color should an object appear with on the radar. Default is white.
    /// </summary>
    /// <param name="ownerLayer">The game object layer of an object.</param>
    /// <returns></returns>
    public static Color GetColor(int ownerLayer)
    {
        if (teamColors.TryGetValue(ownerLayer, out var color)) return color;
        return Color.white;
    }
    #endregion

    #region Teams
    public static (Color color, string colorTag) GetTeamColor(int team)
    {
        return team switch
        {
            1 => (Color.blue, "blue"),
            2 => (Color.red, "red"),
            _ => (Color.white, "white"),
        };
    }

    static readonly Dictionary<int, int> layerTeamCorrespondence = new()
    {
        { 6,1 },{7,1 }, {8,2 },{ 9,2 },{10,10},{0,0}
    };

    public static int EnemyTeam => 2;

    public static int PlayerTeam => 1;

    public static int GetTeam(int objectLayer)
    {
        return layerTeamCorrespondence[objectLayer];
    }
    #endregion
}
