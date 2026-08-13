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
/// Fires when an HPComponent reaches 0 hp.
/// </summary>
public readonly struct ObjectDestroyed : IEvent
{
    public readonly int VictimID, KillerID, VictimTeam, KillerTeam;
    public readonly string VictimName, KillerName;

    public ObjectDestroyed(Transform victim, Transform killer)
    {
        VictimID = victim.GetInstanceID();
        VictimName = victim.name.Replace("(Clone)", "");
        VictimTeam = GlobalConfig.GetTeam(victim.gameObject.layer);
        KillerID = killer.GetInstanceID();
        KillerName = killer.name.Replace("(Clone)", "");
        KillerTeam = GlobalConfig.GetTeam(killer.gameObject.layer);
    }

    public ObjectDestroyed(Transform victim, Unit killer)
    {
        VictimID = victim.GetInstanceID();
        VictimName = victim.name.Replace("(Clone)", "");
        VictimTeam = GlobalConfig.GetTeam(victim.gameObject.layer);
        KillerID = killer.Transform.GetInstanceID();
        KillerName = killer.Transform.name.Replace("(Clone)", "");
        KillerTeam = GlobalConfig.GetTeam(killer.gameObject.layer);
    }

    public override string ToString()
    {
        if (VictimTeam == 0)
        {
            Debug.Log($"{VictimName} was neutral.");
        }
        return $"<color={GlobalConfig.GetTeamColor(VictimTeam).colorTag}>{VictimName} " +
            $"<color=black>destroyed by <color={GlobalConfig.GetTeamColor(KillerTeam).colorTag}>{KillerName}";
    }
}

public readonly struct UnitDestroyed : IEvent
{
    public readonly int UnitTeam;
    public readonly float UnitMoraleWeight;
    public readonly Vector3 UnitPosition;
    public readonly float Time;
    public UnitDestroyed(Unit unit)
    {
        UnitTeam = unit.Team;
        UnitMoraleWeight = unit.GetMoraleWeight();
        UnitPosition = unit.Position;
        Time = UnityEngine.Time.time;
    }
}

public readonly struct ShowText : IEvent
{
    public readonly Vector2 AnchorMax, AnchorMin;
    public readonly Vector3 AnchoredPosition;
    public readonly string Text;
    public readonly Color TextColor;
    public readonly float LifeTime;

    public ShowText(Vector3 anchoredPosition, Vector2 anchorMax, Vector2 anchorMin,
        float lifeTime, string text, Color color)
    {
        AnchoredPosition = anchoredPosition;
        LifeTime = lifeTime;
        AnchorMax = anchorMax;
        AnchorMin = anchorMin;
        Text = text;
        TextColor = color;
    }

    public ShowText(Vector3 anchoredPosition, float lifeTime, string text, Color color)
    {
        AnchoredPosition = anchoredPosition;
        LifeTime = lifeTime;
        AnchorMax = new(0.5f, 0.5f);
        AnchorMin = new(0.5f, 0.5f);
        Text = text;
        TextColor = color;
    }
}
#endregion