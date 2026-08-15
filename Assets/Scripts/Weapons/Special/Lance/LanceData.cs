using AudioSystem;
using UnityEngine;

namespace Weapons
{
    [CreateAssetMenu(menuName = "Weapons/LanceData", fileName = "LanceData")]
    public class LanceData : ProjectileData
    {
        public Material TelegraphMaterial, DamageMaterial;
        [Tooltip("The time between starting to telegraph and dealing damage. This is added to the lifetime of the bullet.")]
        public float Delay = 2;
        public float Range = 200, Radius = 10;
        [Tooltip("The lance will track the target until the timer's progress reaches this value. 0 means it will track the target all the time, 1 means not at all.")]
        [Range(0, 1)]
        public float TimerProgressTrackingThreshold = .5f;
        public SoundData TelegraphSound, FireSound;
        public override float Speed => Range + Delay;
    }
}