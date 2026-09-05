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
    [Tooltip("How many Threats can this AI account for at the same time.")]
    [SerializeField] protected int maxThreats = 1;
    [Tooltip("Only friendly units with an escort value higher than this are eligible for escort duty.")]
    [SerializeField] protected float minEscortValue = 10;

    #region Setup
    protected override void Awake()
    {
        base.Awake();
        if (affectedByMorale) EventBus<UnitDestroyed>.AddActions(OnUnitDestroyed);
        ComponentRegister<BanditTree>.Register(transform, this);
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetTransformInstanceID(Context.Navigation), null, Pause);
    }

    protected override void SetupBlackboard()
    {
        Debug.Assert(minEscortValue > Context.Navigation.EscortValue, $"The min escort value ({minEscortValue}) is less than or equal to the unit's escort value ({Context.Navigation.EscortValue}). The unit will misbehave if it has an escort node.");
        Context.SetData(Blackboard.Keys.MinEscortValue, minEscortValue);
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

    protected override void OnDisable()
    {
        base.OnDisable();
        if (affectedByMorale) EventBus<UnitDestroyed>.RemoveActions(OnUnitDestroyed);
    }

    protected virtual void OnDestroy()
    {
        EventBus<ObjectDestroyed>.RemoveActions(Extensions.GetTransformInstanceID(Context.Navigation), null, Pause);
        ComponentRegister<BanditTree>.Unregister(transform);
    }
    #endregion

    public void SetTarget(Unit target) => blackboard.SetData(Blackboard.Keys.Target, target);

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

        if (@event.VictimTeam == Context.Navigation.Team)
        {
            morale -= moraleImpact;
            Context.SetData(Blackboard.Keys.Morale, morale);
            Context.SetData(Blackboard.Keys.LastMoraleDamageTime, @event.Time);
        }
        else if (@event.VictimTeam != GlobalConfig.NeutralTeam)
        {
            morale += moraleImpact;
            Context.SetData(Blackboard.Keys.Morale, morale);
        }
    }

    public void ReceiveThreat(IThreat threat)
    {
        if (Context.Threats.Count < maxThreats)
        {
            Context.Threats.Add(threat);
            threat.OnThreatOver += RemoveThreat;
        }
    }

    public void RemoveThreat(IThreat threat) => Context.Threats.Remove(threat);
}
