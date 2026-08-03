using UnityEngine;

namespace HybridBT2
{
    [CreateAssetMenu(menuName = "HybridBT2/Nodes/UtilityWrapper")]
    public class UtilityWrapperData : DecoratorData
    {
        public Consideration Consideration;

        protected override Node GetNodeInternal()
        {
            return new UtilityWrapper(Name, Consideration, onEnter, onExit);
        }

        protected void OnValidate()
        {
            Debug.Assert(Consideration != null, $"Consideration is not assigned in {name}. Please assign a valid Consideration.");
        }
    }
}