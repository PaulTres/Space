using UnityEngine;

namespace HybridBT2
{
    [CreateAssetMenu(menuName = "HybridBT2/Decorators/TimeLimit", fileName = "TimeLimit")]
    public class TimeLimitDecoratorData : DecoratorData
    {
        [Tooltip("Maximum time in seconds that an action is allowed to execute for.")]
        public float MaxTime = 60;

        protected override Node GetNodeInternal()
        {
            return new TimeLimit(Name, MaxTime, onEnter, onExit);
        }
    }
}