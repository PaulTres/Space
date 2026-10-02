using AudioSystem;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "ScriptableObjects/GlobalConfig", fileName = "GlobalConfig")]
public class GlobalConfig : ScriptableObject
{
    #region Access
    static readonly string addressableAdress = "GlobalConfig";
    static GlobalConfig instance;
    /// <summary>
    /// Returns a global instance. Will try to find the asset in the addressables 
    /// system if it hasn't been loaded yet. This is a blocking call, so use 
    /// LoadInstance() instead if you want to load it asynchronously.
    /// </summary>
    public static GlobalConfig Instance
    {
        get
        {
            if (instance == null)
            {
                var handle = Addressables.LoadAssetAsync<GlobalConfig>(addressableAdress);
                instance = handle.WaitForCompletion();
            }
            return instance;
        }
    }

    public static async Task LoadInstance()
    {
        var handle = Addressables.LoadAssetAsync<GlobalConfig>(addressableAdress);
        instance = await handle.Task;
    }
    #endregion

    #region Audio
    [Header("Audio")]
    [SerializeField] protected AudioMixer audioMixer;
    public static AudioMixer AudioMixer => Instance.audioMixer;
    #endregion

    #region UI
    /// <summary>
    /// Returns what color should an object appear with on the radar. Default is white.
    /// </summary>
    /// <param name="ownerLayer">The game object layer of an object.</param>
    /// <returns></returns>
    public static Color GetColor(int ownerLayer)
    {
        return ownerLayer switch
        {
            0 => Color.white,
            6 => Color.blue,
            7 => Color.blue,
            8 => Color.red,
            9 => Color.red,
            _ => Color.white
        };
    }

    [Header("UI")]
    [SerializeField] protected SoundData clickSound;
    public static SoundData ClickSound => Instance.clickSound;
    public static float FOV { get; set; } = 90;
    public static float PlayerTargetAngleWeight { get; set; } = 1;
    public static float PlayerTargetDistanceWeight { get; set; } = 1;
    public static float PlayerTargetMaxDistance { get; set; } = 1000;

    [SerializeField] protected float iconSizeCoefficient = 5;
    public static float IconSizeCoefficient => Instance.iconSizeCoefficient;
    #endregion

    #region Controls

    #region Sensitivity
    public static int XSensitivity { get; set; } = 10;
    static readonly string xSensKey = "xSens";
    public static int YSensitivity { get; set; } = 10;
    static readonly string ySensKey = "ySens";
    public static int ZSensitivity { get; set; } = 50;
    static readonly string zSensKey = "zSens";

    public static void LoadSensitivitySettings()
    {
        XSensitivity = PlayerPrefs.GetInt(xSensKey, 10);
        YSensitivity = PlayerPrefs.GetInt(ySensKey, 10);
        ZSensitivity = PlayerPrefs.GetInt(zSensKey, 50);
    }

    public static void SaveSensitivitySettings()
    {
        PlayerPrefs.SetInt(xSensKey, XSensitivity);
        PlayerPrefs.SetInt(ySensKey, YSensitivity);
        PlayerPrefs.SetInt(zSensKey, ZSensitivity);
    }
    #endregion

    #region Invert Y
    public static int YMultiplier => InvertY ? 1 : -1;
    static readonly string invertYKey = "invertY";
    public static bool InvertY { get; set; } = false;

    public static void LoadInvertYSetting()
    {
        InvertY = PlayerPrefs.GetInt(invertYKey, 0) == 1;
    }

    public static void SaveInvertYSetting()
    {
        PlayerPrefs.SetInt(invertYKey, InvertY ? 1 : 0);
    }

    #endregion

    #endregion

    #region Misc

    #region Fleeing
    /// <summary>
    /// It will take this amount of time for bounding boxes of fleeing enemies to 
    /// switch between on and off.
    /// </summary>
    public static float FlickerInterval => Instance.flickerInterval;
    [SerializeField] protected float flickerInterval = 1;
    #endregion

    /// <summary>
    /// When this ship is spawned, no other ships should be around it in this radius.
    /// </summary>
    public static readonly float ShipSpawnEmptySphereRadius = 5;
    /// <summary>
    /// How many times do we try to spawn a ship until the system gives up.
    /// </summary>
    public static readonly int MaxSpawnRetries = 100;
    public static readonly string PlayerOptionsAddressableTag = "Player",
         AllyUnitsAddressableTag = "Allies", EnemyUnitsAddressableTag = "Enemies";
    public static readonly float PositionOscillationCoefficient = 1;
    public static readonly float MaxOscillationMagnitude = 1;
    /// <summary>
    /// Counter-force applied when a ship is above the velocity soft cap will be multiplied by this value.
    /// </summary>
    public static readonly float VelocitySoftCapCoefficient = .5f;
    /// <summary>
    /// Directory where saves are stored. This is the persistent data path, which is platform dependent.
    /// </summary>
    public static string SaveFileLocation => Application.persistentDataPath;
    /// <summary>
    /// This is the size that the missileBuffer is initialized with when checking for missiles.
    /// Might be worth expanding to 100.
    /// </summary>
    public static readonly int MaxMissileCheckBufferSize = 50;
    public static readonly int MaxLanceCheckBufferSize = 50;
    /// <summary>
    /// How many seconds to wait between ending the mission and loading the end mission screen upon mission success.
    /// </summary>
    public static readonly float MissionSuccessEndDuration = 7;
    /// <summary>
    /// How many seconds to wait between ending the mission and loading the end mission screen upon mission failure.
    /// </summary>
    public static readonly float MissionFailureEndDuration = 3;
    public static readonly int
        MainMenuSceneIndex = 1,
        MissionSceneIndex = 2,
        EndMissionSceneIndex = 3,
        TutorialSceneIndex = 4,
        CreditsSceneIndex = 5;
    #endregion

    #region Layers and layer masks
    /// <summary>
    /// This layer mask is used to check for obstructions on spawn.
    /// </summary>
    public static LayerMask SpawnCheckLayerMask => 1 << 0 | 1 << 11;
    /// <summary>
    /// Returns the mask that a lance should use to check for targets.
    /// </summary>
    /// <returns></returns>
    public static LayerMask LanceCheckMask => 1 << 0 | 1 << 6 | 1 << 8;

    public static int GetCollisionLayer(int unitTeam)
    {
        if (unitTeam == PlayerTeam) return 11;
        if (unitTeam == EnemyTeam) return 12;
        return 0;
    }

    /// <summary>
    /// Returns the layer mask that an object should use to check for enemy bullets.
    /// </summary>
    /// <param name="checkerTeam"></param>
    /// <returns></returns>
    public static LayerMask GetEnemyBulletsCheckMask(int checkerTeam)
    {
        if (checkerTeam == PlayerTeam) return 1 << 9 | 1 << 10;
        if (checkerTeam == EnemyTeam) return 1 << 7 | 1 << 10;
        return 1 << 10 | 1 << 7 | 1 << 9;
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
    /// <param name="ownerTeam"></param>
    /// <returns></returns>
    public static int GetBulletLayer(int ownerTeam)
    {
        return ownerTeam switch
        {
            1 => 7,
            2 => 9,
            _ => 10
        };
    }

    /// <summary>
    /// Returns the layer mask that a turret should use to check for obstructions. This is 
    /// used to prevent turrets from trying to shoot through asteroids and other objects.
    /// </summary>
    /// <param name="shooterTeam">The team of the turret object</param>
    /// <returns></returns>
    public static LayerMask GetTurretObstructionMask(int shooterTeam)
    {
        return 1 << 0;
    }

    /// <summary>
    /// Returns what layer a ship should be on, given its Team. Team 1 is layer 6, Team 2 is layer 8.
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

    #region Teams
    public static (Color color, string colorTag) GetTeamColor(int team)
    {
        //Debugging.Log(team);
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

    /// <summary>
    /// 0
    /// </summary>
    public static int NeutralTeam = 0;
    /// <summary>
    /// 1
    /// </summary>
    public static int PlayerTeam => 1;
    /// <summary>
    /// 2
    /// </summary>
    public static int EnemyTeam => 2;

    public static int GetTeam(int objectLayer)
    {
        return layerTeamCorrespondence[objectLayer];
    }
    #endregion

    #region Difficulty
    [Header("Difficulty")]

    [SerializeField] protected List<float> allyHPModifiers = new() { 1.25f, 1, 1f };
    [SerializeField] protected List<float> enemyHPModifiers = new() { .75f, 1, 1.25f };
    public static float GetHPModifier(int team)
    {
        if (team == PlayerTeam) return Instance.allyTrackingModifiers[GameManager.Difficulty];
        if (team == EnemyTeam) return Instance.enemyTrackingModifiers[GameManager.Difficulty];
        return 1;
    }

    public static float GetTrackingModifier(int team)
    {
        if (team == PlayerTeam) return Instance.allyTrackingModifiers[GameManager.Difficulty];
        if (team == EnemyTeam) return Instance.enemyTrackingModifiers[GameManager.Difficulty];
        return 1;
    }
    [SerializeField] protected List<float> allyTrackingModifiers = new() { 1.25f, 1, 1f };
    [SerializeField] protected List<float> enemyTrackingModifiers = new() { .75f, 1, 1.25f };

    public static float GetDamageModifier(int team)
    {
        if (team == PlayerTeam) return Instance.allyDamageModifiers[GameManager.Difficulty];
        if (team == EnemyTeam) return Instance.enemyDamageModifiers[GameManager.Difficulty];
        return 1;
    }
    [SerializeField] protected List<float> allyDamageModifiers = new() { 1.25f, 1, 1f };
    [SerializeField] protected List<float> enemyDamageModifiers = new() { .75f, 1, 1.25f };

    public static float GetUnitSpeedModifier(int team)
    {
        if (team == PlayerTeam) return Instance.allySpeedModifiers[GameManager.Difficulty];
        if (team == EnemyTeam) return Instance.enemySpeedModifiers[GameManager.Difficulty];
        return 1;
    }
    [SerializeField] protected List<float> allySpeedModifiers = new() { 1.25f, 1, 1f };
    [SerializeField] protected List<float> enemySpeedModifiers = new() { .75f, 1, 1.25f };

    public static float GetProjectileSpeedModifier(int team)
    {
        if (team == PlayerTeam) return Instance.allyProjectileSpeedModifiers[GameManager.Difficulty];
        if (team == EnemyTeam) return Instance.enemyProjectileSpeedModifiers[GameManager.Difficulty];
        return 1;
    }
    [SerializeField] protected List<float> allyProjectileSpeedModifiers = new() { 1.25f, 1, 1f };
    [SerializeField] protected List<float> enemyProjectileSpeedModifiers = new() { .75f, 1, 1.25f };

    public static float GetScanRangeModifier(int team)
    {
        if (team == PlayerTeam) return Instance.allyScanRangeModifiers[GameManager.Difficulty];
        if (team == EnemyTeam) return Instance.enemyScanRangeModifiers[GameManager.Difficulty];
        return 1;
    }
    [SerializeField] protected List<float> allyScanRangeModifiers = new() { 1.1f, 1, 1f };
    [SerializeField] protected List<float> enemyScanRangeModifiers = new() { .9f, 1, 1.1f };

    public static float GetSignatureModifier(int team)
    {
        if (team == PlayerTeam) return Instance.allySignatureModifiers[GameManager.Difficulty];
        if (team == EnemyTeam) return Instance.enemySignatureModifiers[GameManager.Difficulty];
        return 1;
    }
    [SerializeField] protected List<float> allySignatureModifiers = new() { 1.1f, 1, 1f };
    [SerializeField] protected List<float> enemySignatureModifiers = new() { .9f, 1, 1.1f };

    public static string DifficultyText(int difficulty)
    {
        return difficulty switch
        {
            0 => "Easy",
            1 => "Normal",
            2 => "Hard",
            _ => "Undefined difficulty"
        };
    }
    #endregion
}
