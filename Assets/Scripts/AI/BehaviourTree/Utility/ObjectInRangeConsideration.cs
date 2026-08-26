using UnityEngine;
using Utilities;
namespace HybridBT2
{
    [CreateAssetMenu(menuName = "HybridBT2/Considerations/ObjectInRange")]
    public class ObjectInRangeConsideration : CurveConsideration
    {
        [SerializeField] protected float maxRange = 1000;
        [Tooltip("The key used to get the object from the blackboard. Must be a derivate of ImprovedMonoBehaviour.")]
        [SerializeField] protected Blackboard.Keys objectKey = Blackboard.Keys.Target;

        protected override float GetValueForCurve(Blackboard context)
        {
            var t = context.GetData<ImprovedMonoBehaviour>(objectKey);
            if (!t.Active()) return 0;
            return Mathf.Clamp01((maxRange - Vector3.Distance(context.Navigation.Position, t.Position)) / maxRange);
        }
    }
}
