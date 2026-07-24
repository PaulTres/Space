using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public static class UnitManager
{
    /// <summary>
    /// All active ships. The key is a ship's Team.
    /// </summary>
    readonly static Dictionary<int, List<Unit>> ships = new();
    public static int AllyCount => ships[GlobalConfig.PlayerTeam].Count;
    public static int EnemyCount => ships[GlobalConfig.EnemyTeam].Count;

    /// <summary>
    /// Get all the ships belonging to a specific team.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public static List<Unit> GetShips(int team)
    {
        return ships.ContainsKey(team) ? ships[team].ToList() : null;
    }

    public static List<Unit> GetTargets(int myTeam)
    {
        return ships.Where(kv => kv.Key != myTeam).SelectMany(kv => kv.Value).ToList();
    }

    public static void Register(Unit ship)
    {
        if (ship == null)
        {
            Debug.LogError("Cannot register null ship");
            return;
        }
        int team = ship.Team;
        if (!ships.ContainsKey(team))
        {
            ships[team] = new();
        }
        ships[team].Add(ship);
        ship.SetObjectLayer(GlobalConfig.GetShipLayer(team));
        //ship.gameObject.layer = GlobalConfig.GetShipLayer(team);
        ship.UnitManagerIndex = ships[team].Count - 1;
        //Debug.Log($"Registered unit {ship.Transform} with index {ship.UnitManagerIndex}.");
    }

    public static void Unregister(Unit ship)
    {
        if (ship == null || ship.UnitManagerIndex == -1) return;
        int team = ship.Team;
        if (ships.ContainsKey(team))
        {
            int index = ship.UnitManagerIndex;
            ships[team].RemoveAtSwapBack(ship.UnitManagerIndex);
            if (ships[team].Count > index) ships[team][index].UnitManagerIndex = index;
            //ship.UnitManagerIndex = -1;
        }

        if (ships[GlobalConfig.EnemyTeam].Count == 0 && Mission.CurrentMission != null)
        {
            Mission.CurrentMission.OnAllEnemiesDestroyed();
        }
    }
}
