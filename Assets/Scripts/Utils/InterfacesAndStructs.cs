using EventBus;
using UnityEngine;
using Utilities;

/// <summary>
/// Provides linear and angular velocity.
/// </summary>
public interface IObject : IRegisterableComponent
{
    public enum Type
    {
        Missile,
        Skiff
    }

    public Type ObjectType { get; }
    public bool Active
    {
        get
        {
            if (Transform == null) return false;
            return Transform.gameObject.activeSelf;
        }
    }
    float Signature { get; }
    Transform Transform { get; }
    public Vector3 Forward { get; }
    public Vector3 Right { get; }
    public Vector3 Up { get; }
    public Vector3 Position => Transform.position;
    Vector3 AngularVelocity { get; }
    Vector3 LinearVelocity { get; }
    public virtual IObject Target => null;
}

public struct DamageInfo : IEvent
{
    public float Amount { get; set; }
    public Transform Source { get; set; }
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

    public override string ToString()
    {
        return $"<color={GlobalConfig.GetTeamColor(VictimTeam).colorTag}>{VictimName} " +
            $"<color=black>destroyed by <color={GlobalConfig.GetTeamColor(KillerTeam).colorTag}> KillerName";
    }
}

public readonly struct PoolableRecycled : IEvent { }

public interface IResettable
{
    void PerformReset();
}

public interface IOwnable : IRegisterableComponent
{
    Transform Owner { get; }
}