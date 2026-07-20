using EventBus;
using HybridBT2;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;
public class BanditTree : BT, IRegisterableComponent
{
    public Blackboard Blackboard => blackboard;
    [SerializeField] protected List<Turret> pointDefenseTurrets = new();
    [SerializeField] protected List<MissileLauncher> missileLaunchers = new();
    [SerializeField] protected float initialMorale = 10;
    [SerializeField] protected float maxMoraleImpactRange = 50;

    protected override void Awake()
    {
        base.Awake();
        EventBus<UnitDestroyed>.AddActions(OnUnitDestroyed);
        ComponentRegister<BanditTree>.Register(transform, this);
    }

    /// <summary>
    /// Fires when a unit is destroyed on the field. Increases morale if an enemy 
    /// was destroyed and decreases it if a friendly was destroyed. Morale impact 
    /// is affected by distance.
    /// </summary>
    /// <param name="event"></param>
    protected void OnUnitDestroyed(UnitDestroyed @event)
    {
        //this should increase morale if an enemy unit is destroyed, and
        //decrease it if an ally is destroyed
        var morale = Blackboard.GetData<float>(Blackboard.Keys.Morale);
        var dist = Vector3.Distance(@event.UnitPosition, transform.position);
        float moraleImpact = @event.UnitMoraleWeight * (Mathf.Max(
            maxMoraleImpactRange - dist, 0) / maxMoraleImpactRange);

        if (@event.UnitTeam == Blackboard.Unit.Team)
        {
            morale -= moraleImpact;
            Blackboard.SetData(Blackboard.Keys.Morale, morale);
            Blackboard.SetData(Blackboard.Keys.LastMoraleDamageTime, @event.Time);
        }
        else if (@event.UnitTeam != GlobalConfig.NeutralTeam)
        {
            morale += moraleImpact;
            Blackboard.SetData(Blackboard.Keys.Morale, morale);
        }
    }

    protected virtual void OnDestroy()
    {
        EventBus<UnitDestroyed>.RemoveActions(OnUnitDestroyed);
        ComponentRegister<BanditTree>.Unregister(transform);
    }

    public void SetTarget(Unit target) => blackboard.SetData(Blackboard.Keys.Target, target);

    protected override void SetupBlackboard()
    {
        Blackboard.SetData(Blackboard.Keys.Fleeing, false);
        Blackboard.SetData(Blackboard.Keys.Launchers, missileLaunchers);
        Blackboard.SetData(Blackboard.Keys.TargetPosition, Vector3.zero);
        Blackboard.SetData(Blackboard.Keys.PDTurrets, pointDefenseTurrets);
        Blackboard.SetData(Blackboard.Keys.LastMoraleDamageTime, Time.time);
        Blackboard.SetData(Blackboard.Keys.DefaultMorale, initialMorale);
        Blackboard.SetData(Blackboard.Keys.Morale, initialMorale);
    }
}
