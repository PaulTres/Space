using HybridBT2;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Weapons;
public class BanditTree : BT, IRegisterableComponent
{
    [SerializeField] protected List<Turret> pointDefenseTurrets = new();
    public Blackboard Blackboard => blackboard;

    protected override void Awake()
    {
        base.Awake();
        ComponentRegister<BanditTree>.Register(transform, this);
    }

    protected virtual void OnDestroy()
    {
        ComponentRegister<BanditTree>.Unregister(transform);
    }

    public void SetTarget(Unit target) => blackboard.SetData(Blackboard.Keys.Target, target);

    protected override void SetupBlackboard()
    {
        var launchers = GetComponentsInChildren<MissileLauncher>();
        Blackboard.SetData(Blackboard.Keys.Launchers, launchers);
        Blackboard.SetData(Blackboard.Keys.TargetPosition, Vector3.zero);
        Blackboard.SetData(Blackboard.Keys.PDTurrets, pointDefenseTurrets);
    }
}
