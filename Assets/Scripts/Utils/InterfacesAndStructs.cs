using EventBus;
using UnityEngine;
using Utilities;

/// <summary>
/// Provides linear and angular velocity, transform and a bunch of other bullshit.
/// </summary>
public interface IObject : IRegisterableComponent
{
    public enum Type
    {
        Missile,
        //tiny baby ship
        Skiff,
        //smallest actual line vessel
        Frigate,
        //actual line vessel
        Cruiser
    }

    public Type ObjectType { get; }
    /// <summary>
    /// Returns true if the transform property is not null and the game object is active.
    /// </summary>
    public bool Active
    {
        get
        {
            if (Transform == null) return false;
            return Transform.gameObject.activeSelf;
        }
    }
    float CurrentSignature { get; }
    Transform Transform { get; }
    public Vector3 Forward { get; }
    public Vector3 Right { get; }
    public Vector3 Up { get; }
    public Vector3 Position => Transform.position;
    Vector3 AngularVelocity { get; }
    Vector3 LinearVelocity { get; }
    public virtual IObject Target => null;
}

public interface IResettable
{
    void PerformReset();
}

public interface IOwnable : IRegisterableComponent
{
    Unit Owner { get; }
}

#region Event bus stuff
public struct DamageInfo : IEvent
{
    public float Amount { get; set; }
    public Unit Source { get; set; }
}

public readonly struct ObjectHPChanged : IEvent
{
    public readonly float CurrentHP, CurrentHPPercentage;

    public ObjectHPChanged(float currentHP, float currentHPPercentage)
    {
        CurrentHP = currentHP;
        CurrentHPPercentage = currentHPPercentage;
    }
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

public readonly struct PoolableRecycled : IEvent { }

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