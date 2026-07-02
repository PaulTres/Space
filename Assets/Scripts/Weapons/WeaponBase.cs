using Timers;
using UnityEngine;
using Utilities;
namespace Weapons
{
    public abstract class WeaponBase : MonoBehaviour, IResettable
    {
        #region Utils   
        protected Unit unit;
        public Unit Unit
        {
            get
            {
                if (unit == null)
                {
                    unit = transform.root.GetComponent<Unit>();
                }
                return unit;
            }
        }
        protected CountdownTimer shotTimer;
        protected float currentShotCooldown;
        #endregion

        #region Weapon parameters
        [SerializeField] protected WeaponParameters @params;
        [SerializeField] protected Transform[] shootPoints;
        public BulletData BulletData => @params != null ? @params.BulletData : null;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            Debug.Assert(@params != null, $"{transform} has no weapon parameters!");
            Debug.Assert(@params.MaxShotCooldown >= 0, $"{transform} has negative cooldown!");
            Debug.Assert((!shootPoints.IsEmpty() && shootPoints[0] != null), $"{transform} has invalid shoot points!");
            shotTimer = new(@params.MaxShotCooldown);
        }

        protected virtual void OnEnable()
        {
            PerformReset();
        }

        /// <summary>
        /// Reset cooldown.
        /// </summary>
        public virtual void PerformReset()
        {
            shotTimer.Reset(@params.MaxShotCooldown);
        }

        protected virtual void OnDestroy()
        {
            shotTimer.Dispose();
        }
        #endregion

        #region Functionality
        public virtual void IncreaseReadiness(float deltaTime)
        {
            currentShotCooldown = Mathf.Max(@params.MinShotCooldown,
                currentShotCooldown - @params.RateOfFireRampUp);
        }

        public virtual void DecreaseReadiness(float deltaTime)
        {
            currentShotCooldown = Mathf.Min(@params.MaxShotCooldown,
                currentShotCooldown + @params.RateOfFireRampDown);
        }

        public virtual bool CanShoot()
        {
            return !shotTimer.IsRunning;
        }

        public bool Shoot(IObject target)
        {
            if (!CanShoot()) return false;
            Fire(target);
            shotTimer.Reset(currentShotCooldown);
            shotTimer.Start();
            return true;
        }

        /// <summary>
        /// Fire once.
        /// </summary>
        public virtual void Fire(IObject target)
        {
            for (int i = 0; i < shootPoints.Length; i++)
            {
                ProcessBullet(@params.GetInstance(shootPoints[i]), target);
            }
        }

        public virtual void ProcessBullet(Bullet bullet, IObject target)
        {
            Unit.IncreaseSignature(@params.SignatureIncrease);
            bullet.Owner = transform.root;
            bullet.gameObject.SetActive(true);
        }
        #endregion
    }
}