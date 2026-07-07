using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public static class UnitManager
{
    /// <summary>
    /// All active ships. The key is a ship's team.
    /// </summary>
    readonly static Dictionary<int, List<Unit>> ships = new();

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
        ship.gameObject.layer = GlobalConfig.GetShipLayer(team);
        ship.UnitManagerIndex = ships[team].Count - 1;
    }

    public static void Unregister(Unit ship)
    {
        if (ship == null) return;
        int team = ship.Team;
        int index = ship.UnitManagerIndex;
        if (ships.ContainsKey(team))
        {
            ships[team].RemoveAtSwapBack(ship.UnitManagerIndex);
            if (ships[team].Count > 0) ships[team][index].UnitManagerIndex = index;
            ship.UnitManagerIndex = -1;
        }
    }
}
