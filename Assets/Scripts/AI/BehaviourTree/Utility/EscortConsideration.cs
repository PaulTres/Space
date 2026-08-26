using UnityEngine;
using Utilities;
namespace HybridBT2
{
    [CreateAssetMenu(menuName = "HybridBT2/Considerations/Escort")]
    public class EscortConsideration : CurveConsideration
    {
        [SerializeField] protected float maxRange = 1000;

        protected override float GetValueForCurve(Blackboard context)
        {
            var minEscortValue = context.GetData<float>(Blackboard.Keys.MinEscortValue);
            var allies = context.Navigation.GetAllies();
            var bestCandidate = context.GetData<Unit>(Blackboard.Keys.EscortCandidate);
            var bestScore = EvaluateCandidate(bestCandidate, context, minEscortValue);
            foreach (var ally in allies)
            {
                if (ally.EscortValue < minEscortValue) continue;
                var score = EvaluateCandidate(ally, context, minEscortValue);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = ally;
                }
            }
            context.SetData(Blackboard.Keys.EscortCandidate, bestCandidate);
            return bestScore;
        }

        protected float EvaluateCandidate(Unit candidate, Blackboard context, float minEscortValue)
        {
            if (!candidate.Active()) return 0;
            return Mathf.Clamp01(candidate.EscortValue / (minEscortValue * minEscortValue + Mathf.Epsilon)) * Mathf.Clamp01(maxRange - Vector3.Distance(candidate.Position, context.Position) / maxRange);
        }
    }
}
