using Effects;
using UnityEngine;
namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Missile Data", fileName = "Missile Data")]
    public class MissileData : ProjectileData
    {
        public float Tracking, MaxTargetSignature, MaxTelemetryBonus = 0.7f, HP = 1;
        public Material RendererMaterial;
        public PoolableEffect OnDeathEffect;

        protected override void OnValidate()
        {
            base.OnValidate();
            if (Tracking < 0)
            {
                Debug.LogError($"Missile tracking cannot be <0 for {this}. Setting to 0.");
                Tracking = 0;
            }
        }
    }
}