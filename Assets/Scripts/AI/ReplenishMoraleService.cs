using HybridBT2;
using System;
using UnityEngine;

[CreateAssetMenu(fileName ="ReplenishMorale",menuName = "HybridBT2/Services/ReplenishMorale")]
public class ReplenishMoraleService : ServiceData
{
    [Tooltip("The amount of morale that gets replenished per second once regen starts.")]
    [SerializeField] protected float moraleReplenishmentRate = .2f;
    [Tooltip("The amount of time that must pass after a morale hit for it to begin recharging.")]
    [SerializeField] protected float moraleRechargeCooldown = 10;
    public override Action<Node, Blackboard> Action => (n, ctx) =>
    {
        var time = ctx.GetData<float>(Blackboard.Keys.LastMoraleDamageTime);
        if (Time.time - time < moraleRechargeCooldown) return;
        var morale = ctx.GetData<float>(Blackboard.Keys.Morale);
        var defaultMorale = ctx.GetData<float>(Blackboard.Keys.DefaultMorale);
        if(morale<defaultMorale)
        {
            morale=Mathf.Min(morale+moraleReplenishmentRate*ctx.DeltaTime, 
                defaultMorale);
            ctx.SetData(Blackboard.Keys.Morale, morale);
        }
    };
}
