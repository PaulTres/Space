using Timers;
using UnityEngine;

namespace Weapons
{
    public class Magazine : MonoBehaviour
    {
        [field: SerializeField] public int InitialAmmoCount { get; protected set; } = 50;
        public int AmmoCount { get; protected set; }
        [SerializeField] protected float ammoReplenishmentTime=10;
        [SerializeField] protected int ammoReplenishmentAmount=2;
        protected CountdownTimer ammoReplenishmentTimer;

        protected void Awake()
        {
            ammoReplenishmentTimer = new(ammoReplenishmentTime);
            ammoReplenishmentTimer.OnTimerStop += () =>
            {
                if (AmmoCount <= 0) AmmoCount = ammoReplenishmentAmount;
            };
        }

        protected void OnEnable()
        {
            AmmoCount = InitialAmmoCount;
        }

        public void Consume()
        {
            if (AmmoCount <= 0) return;
            AmmoCount -= 1;
            if(AmmoCount<=0)
            {
                ammoReplenishmentTimer.Start();
            }
        }

        protected void OnDestroy()
        {
            ammoReplenishmentTimer?.Dispose();
        }
    }
}