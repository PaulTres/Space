using AudioSystem;
using EventBus;
using UnityEngine;
using Utilities;

public interface IResettable
{
    void PerformReset();
}

#region Event bus stuff
public struct DamageInfo : IEvent
{
    public float Amount { get; set; }
    public Unit Source { get; set; }
    public bool IgnoreArmor { get; set; }
    public Vector3 Position { get; set; }
}

/// <summary>
/// Fires when an HPComponent reaches 0 hp. Should only be used to trigger death and level events.
/// </summary>
public readonly struct ObjectDestroyed : IEvent
{
    public readonly Unit Killer;
    public readonly int VictimID;

    public ObjectDestroyed(int victimID, Unit killer)
    {
        VictimID = victimID;
        Killer = killer;
        //Debug.Log(Killer == null);
    }

    public override string ToString()
    {
        return "Victim " + VictimID + " was destroyed by " + (Killer != null ? Killer.name : "nothing") + '.';
    }
}

/// <summary>
/// Fires when a unit is destroyed. Used to calculate morale loss and to keep track of kills.
/// </summary>
public readonly struct UnitDestroyed : IEvent
{
    public readonly int VictimTeam;
    public readonly string VictimName;
    public readonly int VictimID;
    public readonly float UnitMoraleWeight;
    public readonly Vector3 UnitPosition;
    public readonly float Time;
    public readonly int KillerTeam;
    public readonly string KillerName;

    public UnitDestroyed(Unit unit, Unit killer)
    {
        VictimTeam = unit.Team;
        VictimID = unit.GetTransformInstanceID();
        VictimName = unit.Transform.name.Replace("(Clone)", "");
        UnitMoraleWeight = unit.GetMoraleWeight();
        UnitPosition = unit.Position;
        Time = UnityEngine.Time.time;
        if (killer != null)
        {
            KillerTeam = killer.Team;
            KillerName = killer.Transform.name.Replace("(Clone)", "");
        }
        else
        {
            KillerTeam = 0;
            KillerName = "";
        }
    }

    public override string ToString()
    {
        return $"<color={GlobalConfig.GetTeamColor(VictimTeam).colorTag}>{VictimName} " +
            $"<color=black>destroyed by <color={GlobalConfig.GetTeamColor(KillerTeam).colorTag}>{KillerName}";
    }
}

public readonly struct ShowText : IEvent
{
    public readonly Vector2 AnchorMax, AnchorMin;
    public readonly Vector3 AnchoredPosition;
    public readonly string Text;
    public readonly Color TextColor;
    public readonly float LifeTime;
    public readonly SoundData Sound;

    public ShowText(Vector3 anchoredPosition, float lifeTime, string text, Color color, SoundData sound = null)
    {
        AnchoredPosition = anchoredPosition;
        LifeTime = lifeTime;
        AnchorMax = new(0.5f, 0.5f);
        AnchorMin = new(0.5f, 0.5f);
        Text = text;
        TextColor = color;
        Sound = sound;
    }
}
#endregion