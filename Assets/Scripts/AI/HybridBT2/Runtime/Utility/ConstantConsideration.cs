using UnityEngine;

namespace HybridBT2
{
    [CreateAssetMenu(menuName = "HybridBT2/Considerations/Constant")]
    public class ConstantConsideration : Consideration
    {
        [SerializeField] protected float value;
        public override float Evaluate(Blackboard context) => value;
    }
}