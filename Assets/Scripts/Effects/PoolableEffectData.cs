using Spawning;
using UnityEngine;
namespace Effects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/EffectData", fileName = "EffectData")]
    public class PoolableEffectData : MonoPoolableData<PoolableEffect.Type, PoolableEffect>
    {
        protected override PoolableEffect GetInstance()
        {
            if (PoolableEffect.Get(Key, out var effect))
            {
                return effect;
            }
            return CreateInstance();
        }
    }
}