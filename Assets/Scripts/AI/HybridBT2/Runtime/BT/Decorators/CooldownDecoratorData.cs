using UnityEngine;

namespace HybridBT2
{
    [CreateAssetMenu(menuName = "HybridBT2/Decorators/Cooldown", fileName = "Cooldown")]
    public class CooldownDecoratorData : DecoratorData
    {
        public float Cooldown = 10;

        protected override Node GetNodeInternal()
        {
            return new CooldownDecorator(Name, Cooldown, onEnter, onExit);
        }
    }
}