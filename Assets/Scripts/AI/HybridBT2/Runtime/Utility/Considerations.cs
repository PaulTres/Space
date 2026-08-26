using UnityEngine;

namespace HybridBT2
{
    public abstract class Consideration : ScriptableObject
    {
        public abstract float Evaluate(Blackboard context);
    }

    public abstract class CurveConsideration : Consideration
    {
        [SerializeField] protected AnimationCurve curve;
        [SerializeField] protected bool returnZeroForInfinity = true;

        protected abstract float GetValueForCurve(Blackboard context);

        public override float Evaluate(Blackboard context)
        {
            var value = GetValueForCurve(context);
            if (returnZeroForInfinity && (value == Mathf.Infinity || value == Mathf.NegativeInfinity)) return 0;
            return Mathf.Clamp01(curve.Evaluate(value));
        }

        protected virtual void Reset()
        {
            curve = new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(1f, 0f)
            );
        }
    }
}