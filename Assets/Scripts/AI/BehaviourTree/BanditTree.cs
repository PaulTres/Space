using EventBus;
using HybridBT2;
using System.Linq;
using UnityEngine;
using Utilities;
using Weapons;
public class BanditTree : BT, IRegisterableComponent
{
    public Blackboard Context => blackboard;
    [SerializeField] protected int maxThreats = 1;
    [Tooltip("Only friendly units with an escort value higher than this are eligible for escort duty.")]
    [SerializeField] protected float minEscortValue = 10;

    #region Setup
    protected override void Awake()
    {
        base.Awake();
        ComponentRegister<BanditTree>.Register(transform, this);
        EventBus<ObjectDestroyed>.AddActions(Extensions.GetTransformInstanceID(Context.Navigation), null, Pause);
    }

    protected void OnValidate()
    {
        var currentEscortValue = GetComponent<Navigation>().EscortValue;
        if (minEscortValue <= currentEscortValue)
        {
            Debug.LogError($"The min escort value ({minEscortValue}) is less than or equal to {transform.root}'s escort value ({currentEscortValue}). The unit will misbehave if it has an escort node. Doubling min escort value for this.");
            minEscortValue = (currentEscortValue + 1) * 2;
        }
    }

    protected override void SetupBlackboard()
    {
        Context.SetData(Blackboard.Keys.MinEscortValue, minEscortValue);
        Context.SetData(Blackboard.Keys.Fleeing, false);
        Context.SetData(Blackboard.Keys.AllTurrets, GetComponentsInChildren<Turret>());
        Context.SetData(Blackboard.Keys.TargetPosition, Vector3.zero);
        Context.SetData(Blackboard.Keys.PDTurrets,
            GetComponentsInChildren<Turret>().
            Where(t => t.Parameters.TargetMissiles));
    }

    protected virtual void OnDestroy()
    {
        EventBus<ObjectDestroyed>.RemoveActions(Extensions.GetTransformInstanceID(Context.Navigation), null, Pause);
        ComponentRegister<BanditTree>.Unregister(transform);
    }
    #endregion

    public void SetTarget(ImprovedMonoBehaviour target) => blackboard.SetData(Blackboard.Keys.Target, target);

    public void ReceiveThreat(IThreat threat)
    {
        if (Context.Threats.Count < maxThreats)
        {
            Context.Threats.Add(threat);
            threat.OnThreatOver += RemoveThreat;
        }
    }

    public void RemoveThreat(IThreat threat) => Context.Threats.Remove(threat);

    protected void OnDrawGizmosSelected()
    {
        if (Context == null) return;
        var e = Context.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.EscortCandidate);
        if (e.Active())
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Context.Navigation.Position, e.Position);
        }
    }
}
