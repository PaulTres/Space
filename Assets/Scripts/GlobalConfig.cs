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

    public static readonly float UIUpdateCooldown = 1;
    public static readonly float PositionOscillationCoefficient = 1;
    public static readonly float MaxOscillationMagnitude = 1;
    /// <summary>
    /// Directory where saves are stored. This is the persistent data path, which is platform dependent.
    /// </summary>
    public static string SaveFileLocation => Application.persistentDataPath;
    /// <summary>
    /// This is the size that the missileBuffer is initialized with when checking for missiles.
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
    /// <summary>
    /// Returns the layer mask that an object should use to check for enemy bullets.
    /// </summary>
    /// <param name="checkerLayer"></param>
    /// <returns></returns>
    public static LayerMask GetEnemyBulletsCheckMask(int checkerLayer)
    {
        return checkerLayer switch
        {
            6 => 1 << 9 | 1 << 10,
            8 => 1 << 7 | 1 << 10,
            _ => 1 << 10 | 1 << 7 | 1 << 9
        };
    }

    /// <summary>
    /// Returns the collision mask that a bullet should use, based on its own layer.
    /// </summary>
    /// <param name="bulletLayer"></param>
    /// <returns></returns>
    public static LayerMask GetBulletCollisionMask(int bulletLayer)
    {
        return bulletLayer switch
        {
            7 => 1 << 0 | 1 << 8,
            9 => 1 << 0 | 1 << 6,
            _ => 1 << 0 | 1 << 6 | 1 << 8
        };
    }

    /// <summary>
    /// Returns a layer that a bullet should use, given the layer of its owner.
    /// </summary>
    /// <param name="ownerLayer"></param>
    /// <returns></returns>
    public static int GetBulletLayer(int ownerLayer)
    {
        return ownerLayer switch
        {
            6 => 7,
            8 => 9,
            _ => 10
        };
    }

    /// <summary>
    /// Returns the layer mask that a turret should use to check for obstructions. This is 
    /// used to prevent turrets from trying to shoot through asteroids and other objects.
    /// </summary>
    /// <param name="shooterLayer">The layer of the turret object</param>
    /// <returns></returns>
    public static LayerMask GetTurretObstructionMask(int shooterLayer)
    {
        return 1 << 0;
    }

    /// <summary>
    /// Returns what layer a ship should be on, given its team. Team 1 is layer 6, team 2 is layer 8.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public static int GetShipLayer(int team)
    {
        return team switch
        {
            1 => 6,
            2 => 8,
            _ => 0
        };
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
