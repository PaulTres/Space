using AudioSystem;
using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/LanceData", fileName = "LanceData")]
    public class LanceData : BulletData
    {
        public Material TelegraphMaterial, DamageMaterial;
        public float Delay = 2;
        public float Range = 200, Radius = 10;
        public SoundData TelegraphSound, FireSound;
        public override float Speed => Delay;
    }
}