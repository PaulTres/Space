using Effects;
using EventBus;
using HybridBT2;
using System;
using UnityEngine;
using Utilities;

[CreateAssetMenu(menuName = "HybridBT2/Leaves/Flee", fileName = "Flee")]
public class FleeNode : LeafNodeData
{
    [Tooltip("Effect to play when jumping. If null, will play the unit's spawn effect.")]
    [SerializeField] protected PoolableEffect jumpEffect;
    [SerializeField] protected float fleeDistance = 1000;
    [Tooltip("This much time must pass in seconds until the unit can jump out of the battle.")]
    [SerializeField] protected int requiredJumpCharge = 5;

    protected override Action<Node, Blackboard> onEnter => (n, ctx) =>
    {
        ctx.Navigation.RotationTarget = null;
        //Debugging.Log($"{ctx.Navigation} is fleeing!");
        ctx.SetData(Blackboard.Keys.Fleeing, true);
        ((LeafNode)n).SetData<float>(0, 0);
    };

    protected override Action<Node, Blackboard> onExit => (n, ctx) =>
    {
        //int myMorale;
        //int enemyMorale;
        //if (ctx.Navigation.Team == GlobalConfig.PlayerTeam)
        //{
        //    myMorale = UnitManager.GetMorale(GlobalConfig.PlayerTeam);
        //    enemyMorale = UnitManager.GetMorale(GlobalConfig.EnemyTeam);
        //}
        //else if (ctx.Navigation.Team == GlobalConfig.EnemyTeam)
        //{
        //    myMorale = UnitManager.GetMorale(GlobalConfig.EnemyTeam);
        //    enemyMorale = UnitManager.GetMorale(GlobalConfig.PlayerTeam);
        //}
        //else throw new Exception($"{ctx.Navigation} is not on a team!");
        //var moraleFleeThreshold = 3;
        //Debugging.Log($"{myMorale * moraleFleeThreshold} vs {enemyMorale}");

        ctx.SetData(Blackboard.Keys.Fleeing, false);
    };

    protected override Func<LeafNode, Blackboard, NodeState> onEvaluate => (n, ctx) =>
    {
        var target = ctx.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        //no need to normalize here, we just need the direction
        //this could probably be optimized to only happen once every few ticks
        if (target.Active())
        {
            ctx.Navigation.SetDestination(ctx.Navigation.Position +
                (ctx.Navigation.Position - target.Position) * fleeDistance);
        }
        var jumpCharge = n.GetData<float>(0);
        if (jumpCharge >= requiredJumpCharge)
        {
            //jump
            if (jumpEffect != null) jumpEffect.SpawnEffect(ctx.Position, ctx.Navigation.EffectScaleFactor);
            else ctx.Navigation.SpawnEffect.SpawnEffect(ctx.Position, ctx.Navigation.EffectScaleFactor);
            EventBus<ObjectDestroyed>.Raise(ctx.Navigation.GetTransformInstanceID(), new(ctx.Navigation.GetTransformInstanceID(), null));
            ctx.Navigation.gameObject.SetActive(false);
        }
        else
        {
            jumpCharge += ctx.DeltaTime;
            n.SetData(0, jumpCharge);
        }
        return NodeState.RUNNING;
    };
}
