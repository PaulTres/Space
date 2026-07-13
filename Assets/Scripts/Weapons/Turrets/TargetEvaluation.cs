using UnityEngine;
namespace Weapons
{
    public abstract class TargetEvaluation : ScriptableObject
    {
        public abstract float Evaluate(Unit shooter, IObject target, float maxRange);

        public abstract float Evaluate(Turret shooter, IObject target);
    }
}