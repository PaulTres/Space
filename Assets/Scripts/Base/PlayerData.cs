using Player;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/PlayerData", fileName = "PlayerData")]
public class PlayerData : UnitData
{

    [Tooltip("A ship cannot be used before this mission has been completed.")]
    public int MissionRequirement = -1;
    public Transform PreviewModel;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (Prefab != null)
        {
            var a = (PlayerShip)Prefab;
            if (a == null) Debug.LogError($"{this} does not have a player ship!");
        }
    }
}