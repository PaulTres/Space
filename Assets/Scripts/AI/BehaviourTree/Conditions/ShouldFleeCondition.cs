using HybridBT2;
using System;
using UnityEngine;


[CreateAssetMenu(fileName = "ShouldFlee", menuName = "HybridBT2/Conditions/ShouldFlee")]
public class ShouldFleeCondition : ConditionData
{
    [Tooltip("Enemy morale needs to be higher than MoraleFleeThreshold*myMorale for the ship to flee.")]
    [SerializeField] protected float moraleFleeThreshold = 3;

    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        int myMorale;
        int enemyMorale;
        if (ctx.Navigation.Team == GlobalConfig.PlayerTeam)
        {
            myMorale = UnitManager.GetMorale(GlobalConfig.PlayerTeam);
            enemyMorale = UnitManager.GetMorale(GlobalConfig.EnemyTeam);
        }
        else if (ctx.Navigation.Team == GlobalConfig.EnemyTeam)
        {
            myMorale = UnitManager.GetMorale(GlobalConfig.EnemyTeam);
            enemyMorale = UnitManager.GetMorale(GlobalConfig.PlayerTeam);
        }
        else throw new Exception($"{ctx.Navigation} is on invalid team {ctx.Navigation.Team}!");
        //if (!(myMorale * moraleFleeThreshold <= enemyMorale)) Debugging.Log($"{myMorale * moraleFleeThreshold} vs {enemyMorale}");
        return myMorale * moraleFleeThreshold <= enemyMorale;
    };
}
