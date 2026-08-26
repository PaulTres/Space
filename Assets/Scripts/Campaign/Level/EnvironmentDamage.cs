using Effects;
using UnityEngine;
using Utilities;

namespace Level
{
    public class EnvironmentDamage : Singleton<EnvironmentDamage>
    {
        [SerializeField] protected float damage = 1, interval = 1;
        [SerializeField] protected PoolableEffect onDamageEffect;
        public float Interval => interval;
        protected DamageInfo damageInfo;

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            damageInfo.Amount = damage;
        }

        public void DealDamage(Unit unit)
        {
            unit.Transform.TakeDamage(damageInfo);
            if (onDamageEffect != null)
            {
                onDamageEffect.SpawnEffect(unit.Position, unit.EffectScaleFactor);
            }
        }
    }
}