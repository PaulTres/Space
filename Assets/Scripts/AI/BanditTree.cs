using EventBus;
using HybridBT2;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;
public class BanditTree : BT, IRegisterableComponent
{
    [SerializeField] protected List<Turret> pointDefenseTurrets = new();
    public Blackboard Blackboard => blackboard;
    [SerializeField] protected float initialMorale = 10;

    protected override void Awake()
    {
        base.Awake();
        EventBus<UnitDestroyed>.AddActions(OnUnitDestroyed);
        ComponentRegister<BanditTree>.Register(transform, this);
    }

    protected void OnUnitDestroyed(UnitDestroyed @event)
    {
        //this should increase morale if an enemy unit is destroyed, and
        //decrease it if an ally is destroyed
        var morale = Blackboard.GetData<float>(Blackboard.Keys.Morale);
        if (@event.Unit.Team == Blackboard.Unit.Team)
        {
            morale -= @event.Unit.GetMoraleWeight();
            Blackboard.SetData(Blackboard.Keys.Morale, morale);
            Blackboard.SetData(Blackboard.Keys.LastMoraleDamageTime, @event.Time);
        }
        else if (@event.Unit.Team != GlobalConfig.NeutralTeam)
        {
            morale += @event.Unit.GetMoraleWeight();
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
        var launchers = GetComponentsInChildren<MissileLauncher>();
        Blackboard.SetData(Blackboard.Keys.Fleeing, false);
        Blackboard.SetData(Blackboard.Keys.Launchers, launchers);
        Blackboard.SetData(Blackboard.Keys.TargetPosition, Vector3.zero);
        Blackboard.SetData(Blackboard.Keys.PDTurrets, pointDefenseTurrets);
        Blackboard.SetData(Blackboard.Keys.LastMoraleDamageTime, Time.time);
        Blackboard.SetData(Blackboard.Keys.DefaultMorale, initialMorale);
        Blackboard.SetData(Blackboard.Keys.Morale, initialMorale);
    }
}
