using Effects;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Level
{
    public class EnvironmentDamage : Singleton<EnvironmentDamage>
    {
        [SerializeField] protected float damage = 1, interval = 1;
        [SerializeField] protected PoolableEffect onDamageEffect;
        [Tooltip("The team that will take damage from this.")]
        [SerializeField] protected int[] targetTeams = new int[] { 1 };
        public float Interval => interval;
        protected DamageInfo damageInfo;
        protected Dictionary<Unit, float> damageIntervals = new();

        protected override void InitializeSingleton()
        {
            base.InitializeSingleton();
            damageInfo.Amount = damage;
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformFixedUpdate);
        }

        protected override void ClearSingleton()
        {
            base.ClearSingleton();
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterFixedUpdate(PerformFixedUpdate);
        }

        protected void PerformFixedUpdate(float deltaTime)
        {
            for (int i = 0; i < targetTeams.Length; i++)
            {
                foreach (var unit in UnitManager.GetShips(targetTeams[i]))
                {
                    if (!unit.Active()) continue;
                    if (SafeZone.InSafeZone(unit))
                    {
                        damageIntervals[unit] = 0;
                    }
                    else
                    {
                        if (!damageIntervals.TryAdd(unit, 0))
                        {
                            var value = damageIntervals[unit] + deltaTime;
                            if (value > interval)
                            {
                                value = 0;
                                DealDamage(unit);
                            }
                            damageIntervals[unit] = value;
                        }
                    }
                }
            }
        }

        protected void DealDamage(Unit unit)
        {
            //Debug.Log($"Damaging {unit}");
            unit.Transform.TakeDamage(damageInfo);
            if (onDamageEffect != null)
            {
                onDamageEffect.SpawnEffect(unit.Position, unit.EffectScaleFactor);
            }
        }
    }
}