using UnityEngine;

namespace HybridBT2
{
    [CreateAssetMenu(menuName = "HybridBT2/Considerations/Composite")]
    public sealed class CompositeConsideration : Consideration
    {
        enum OperationType { Average, Multiply, Add, Subtract, Divide, Max, Min }
        [SerializeField] bool allMustBeNonZero = true;
        [SerializeField] OperationType operation = OperationType.Max;
        [SerializeField] Consideration[] considerations;

        public override float Evaluate(Blackboard context)
        {
            if (considerations == null || considerations.Length == 0) return 0f;

            float result = considerations[0].Evaluate(context);
            if (result == 0f && allMustBeNonZero) return 0f;

            for (int i = 1; i < considerations.Length; i++)
            {
                float value = considerations[i].Evaluate(context);

                if (value == 0f && allMustBeNonZero) return 0f;

                switch (operation)
                {
                    case OperationType.Average:
                        result += value;
                        break;
                    case OperationType.Multiply:
                        result *= value;
                        break;
                    case OperationType.Add:
                        result += value;
                        break;
                    case OperationType.Subtract:
                        result -= value;
                        break;
                    case OperationType.Divide:
                        result = value != 0 ? result / value : result; // Prevent division by zero
                        break;
                    case OperationType.Max:
                        result = Mathf.Max(result, value);
                        break;
                    case OperationType.Min:
                        result = Mathf.Min(result, value);
                        break;
                }
            }

            return Mathf.Clamp01(result);
        }
    }
}