using EventBus;
using HybridBT2;
using System.Linq;
using UnityEngine;
using Utilities;
using Weapons;
public class BanditTree : BT, IRegisterableComponent
{
    public Blackboard Context => blackboard;
    [SerializeField] protected bool affectedByMorale = true;
    [SerializeField] protected float initialMorale = 10;
    [Tooltip("The maximum distance from a morale event at which this unit will be affected.")]
    [SerializeField] protected float maxMoraleImpactRange = 50;

    protected override void Awake()
    {
        base.Awake();
        if (affectedByMorale) EventBus<UnitDestroyed>.AddActions(OnUnitDestroyed);
        ComponentRegister<BanditTree>.Register(transform, this);
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetInstanceID(Context.Navigation), null, Pause);
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
        var morale = Context.GetData<float>(Blackboard.Keys.Morale);
        var dist = Vector3.Distance(@event.UnitPosition, transform.position);
        float moraleImpact = @event.UnitMoraleWeight * (Mathf.Max(
            maxMoraleImpactRange - dist, 0) / maxMoraleImpactRange);

        if (@event.UnitTeam == Context.Navigation.Team)
        {
            morale -= moraleImpact;
            Context.SetData(Blackboard.Keys.Morale, morale);
            Context.SetData(Blackboard.Keys.LastMoraleDamageTime, @event.Time);
        }
        else if (@event.UnitTeam != GlobalConfig.NeutralTeam)
        {
            morale += moraleImpact;
            Context.SetData(Blackboard.Keys.Morale, morale);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (affectedByMorale) EventBus<UnitDestroyed>.RemoveActions(OnUnitDestroyed);
    }
    protected virtual void OnDestroy()
    {
        EventBus<ObjectDestroyed>.RemoveActions(Extensions.GetInstanceID(Context.Navigation), null, Pause);
        ComponentRegister<BanditTree>.Unregister(transform);
    }

    public void SetTarget(Unit target) => blackboard.SetData(Blackboard.Keys.Target, target);

    protected override void SetupBlackboard()
    {
        Context.SetData(Blackboard.Keys.Fleeing, false);
        Context.SetData(Blackboard.Keys.AllTurrets, GetComponentsInChildren<Turret>());
        Context.SetData(Blackboard.Keys.TargetPosition, Vector3.zero);
        Context.SetData(Blackboard.Keys.PDTurrets,
            GetComponentsInChildren<Turret>().
            Where(t => t.Parameters.TargetMissiles));
        Context.SetData(Blackboard.Keys.LastMoraleDamageTime, Time.time);
        Context.SetData(Blackboard.Keys.DefaultMorale, initialMorale);
        Context.SetData(Blackboard.Keys.Morale, initialMorale);
    }
}
