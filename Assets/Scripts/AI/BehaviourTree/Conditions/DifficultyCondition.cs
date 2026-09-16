using HybridBT2;
using System;
using UnityEngine;

public class DifficultyCondition:ConditionData
{
    [Tooltip("The condition passes if the game's difficulty is between these two values (inclusive).")]
    [SerializeField] protected float minDifficulty = 0, maxDifficulty = 2;
    public override Func<Node, Blackboard, bool> Function => (n, ctx) =>
    {
        return GameManager.Difficulty>=minDifficulty && GameManager.Difficulty<=maxDifficulty;
    };
}
