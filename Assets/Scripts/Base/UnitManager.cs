using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Utilities;

public static class UnitManager
{
    /// <summary>
    /// All active ships. The key is a ship's Team.
    /// </summary>
    readonly static Dictionary<int, List<ImprovedMonoBehaviour>> ships = new();
    readonly static Dictionary<int, int> moraleValues = new();
    public static int GetMorale(int team)
    {
        if (moraleValues.TryGetValue(team, out var value)) return value;
        return 0;
    }
    public static int AllyCount => ships[GlobalConfig.PlayerTeam].Count;
    public static int EnemyCount => ships[GlobalConfig.EnemyTeam].Count;

    public static void Clear() => ships.Clear();

    public static IEnumerable<ImprovedMonoBehaviour> GetAllShips()
    {
        return ships.SelectMany(kv => kv.Value);
    }

    /// <summary>
    /// Get all the ships belonging to a specific team.
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public static List<ImprovedMonoBehaviour> GetShips(int team)
    {
        if (ships.TryGetValue(team, out var value)) return value;
        return new();
    }

    public static void Register(ImprovedMonoBehaviour ship)
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
            moraleValues[team] = 0;
        }
        //Debug.Log($"{ship.Transform} registered on team {team}.");
        ships[team].Add(ship);
        ship.UnitManagerIndex = ships[team].Count - 1;
        ship.RegisterForRadar();
        ship.SetObjectLayer();
        var unit = ship as Unit;
        if (unit != null)
        {
            moraleValues[team] = moraleValues[team] + unit.MoraleWeight;
        }

        //Debug.Log($"Registered unit {ship.Transform} with index {ship.UnitManagerIndex}.");
    }

    /// <summary>
    /// Remove the ship from the list of ships and set its index to -1, otherwise 
    /// it can try to unregister again and that fucks with everything.
    /// </summary>
    /// <param name="ship"></param>
    public static void Unregister(ImprovedMonoBehaviour ship)
    {
        ship.UnregisterForRadar();
        if (!ship.Active(true) || ship.UnitManagerIndex == -1) return;
        int team = ship.Team;
        if (ships.ContainsKey(team))
        {
            int index = ship.UnitManagerIndex;
            ships[team].RemoveAtSwapBack(ship.UnitManagerIndex);
            ship.UnitManagerIndex = -1;
            if (ships[team].Count > index) ships[team][index].UnitManagerIndex = index;
            var unit = ship as Unit;
            if (unit != null)
            {
                moraleValues[team] = moraleValues[team] - unit.MoraleWeight;
            }
        }

        //if (ships[GlobalConfig.EnemyTeam].Count == 0 && Mission.CurrentMission != null)
        //{
        //    Mission.CurrentMission.OnAllUnitsDestroyed();
        //}
    }
}
