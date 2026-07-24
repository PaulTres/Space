using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "ScriptableObjects/GlobalConfig", fileName = "GlobalConfig")]
public class GlobalConfig : ScriptableObject
{
    #region Access
    static readonly string addressableAdress = "GlobalConfig";
    static GlobalConfig instance;
    public static GlobalConfig Instance
    {
        get
        {
            if (instance == null)
            {

                var handle = Addressables.LoadAssetAsync<GlobalConfig>(addressableAdress);
                instance = handle.WaitForCompletion();
                foreach (var a in instance.objectIconsList)
                {
                    instance.objectIconsDict.Add(a.Type, a.Icon);
                }
            }
            return instance;
        }
    }

    public static async Task LoadInstance()
    {
        var handle = Addressables.LoadAssetAsync<GlobalConfig>(addressableAdress);
        instance = await handle.Task;
        foreach (var a in instance.objectIconsList)
        {
            instance.objectIconsDict.Add(a.Type, a.Icon);
        }
    }
    #endregion

    #region UI
    [Header("UI")]
    public static float PlayerTargetAngleWeight { get; set; } = 1;
    public static float PlayerTargetDistanceWeight { get; set; } = 1;
    public static float PlayerTargetMaxDistance { get; set; } = 100;
    [System.Serializable]
    public class ObjectTypeIconCorrespondence
    {
        public IObject.Type Type;
        public Sprite Icon;
    }
    [SerializeField] protected List<ObjectTypeIconCorrespondence> objectIconsList = new();
    protected Dictionary<IObject.Type, Sprite> objectIconsDict { get; } = new();

    public static Sprite GetIcon(IObject.Type type) => Instance.objectIconsDict[type];

    [SerializeField] protected float iconSizeCoefficient = 5;
    public static float IconSizeCoefficient => Instance.iconSizeCoefficient;
    public static readonly float UIUpdateCooldown = 1;

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
    #endregion

    #region Misc
    /// <summary>
    /// It will take this amount of time for bounding boxes of fleeing enemies to 
    /// switch between on and off.
    /// </summary>
    public static readonly float FlickerInterval = 0.5f;
    /// <summary>
    /// Morale needs to be <= to InitialMorale* this value for the unit to flee.
    /// </summary>
    public static readonly float MoraleFleeThreshold = 0;
    /// <summary>
    /// Morale needs to be at least InitialMorale* this value for the unit 
    /// to stop fleeing.
    /// </summary>
    public static readonly float MoraleRecoverThreshold = .5f;
    /// <summary>
    /// Morale will only replenish after this much time has passed since 
    /// the last morale damage.
    /// </summary>
    public static readonly float MoraleRechargeCooldown = 10;
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
    public static readonly int MainMenuSceneIndex = 1, MissionSceneIndex = 3, EndMissionSceneIndex = 4, LobbySceneIndex = 2;
    #endregion

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
        //Debug.Log(team);
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

    public static int NeutralTeam = 0;
    public static int PlayerTeam => 1;
    public static int EnemyTeam => 2;

    public static int GetTeam(int objectLayer)
    {
        return layerTeamCorrespondence[objectLayer];
    }
    #endregion
}
