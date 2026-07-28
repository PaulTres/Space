using Timers;
using UnityEngine;
using Utilities;
namespace Weapons
{
    public abstract class WeaponBase : MonoBehaviour, IResettable
    {
        #region Utils   
        protected Unit unit;
        /// <summary>
        /// The unit that owns this Weapon. Lazily initialized by searching for a Unit 
        /// component in the root of the transform hierarchy.
        /// </summary>
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
        public float CurrentShotCooldown { get; protected set; }
        #endregion

        #region Weapon parameters
        [SerializeField] protected WeaponData @params;
        [SerializeField] protected Transform[] shootPoints;
        public BulletData BulletData => @params != null ? @params.BulletData : null;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            Debug.Assert(@params != null, $"{transform} has no Weapon parameters!");
            Debug.Assert(@params.MaxShotCooldown >= 0, $"{transform} has negative cooldown!");
            Debug.Assert((!shootPoints.IsEmpty() && shootPoints[0] != null), $"{transform} has invalid shoot points!");
            shotTimer = new(@params.MaxShotCooldown);
            CurrentShotCooldown = @params.MaxShotCooldown;
        }

        protected virtual void OnEnable()
        {
            PerformReset();
        }

        /// <summary>
        /// Reset shot cooldown.
        /// </summary>
        public virtual void PerformReset()
        {
            shotTimer.Reset(@params.MaxShotCooldown);
            CurrentShotCooldown = @params.MaxShotCooldown;
        }

        protected virtual void OnDestroy()
        {
            shotTimer.Dispose();
        }
        #endregion

        #region Functionality
        /// <summary>
        /// Increases Weapon rate of fire.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void IncreaseReadiness(float deltaTime)
        {
            CurrentShotCooldown = Mathf.Max(@params.MinShotCooldown,
                CurrentShotCooldown - @params.RateOfFireRampUp * deltaTime);
        }

        /// <summary>
        /// Decreases Weapon rate of fire.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void DecreaseReadiness(float deltaTime)
        {
            CurrentShotCooldown = Mathf.Min(@params.MaxShotCooldown,
                CurrentShotCooldown + @params.RateOfFireRampDown * deltaTime);
        }

        /// <summary>
        /// Checks if the Weapon can shoot by using the shot cooldown timer.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanShoot()
        {
            return !shotTimer.IsRunning;
        }

        /// <summary>
        /// Returns true if the Weapon can shoot and fires a bullet at the target. 
        /// Resets the shot cooldown timer.
        /// </summary>
        /// <param name="target">The target to shoot at.</param>
        /// <returns>True if the Weapon successfully fired a shot, false otherwise.</returns>
        public bool Shoot(IObject target)
        {
            if (!CanShoot()) return false;
            Fire(target);
            shotTimer.Reset(CurrentShotCooldown);
            shotTimer.Start();
            return true;
        }

        /// <summary>
        /// Fire once for every shoot point.
        /// </summary>
        public virtual void Fire(IObject target)
        {
            for (int i = 0; i < shootPoints.Length; i++)
            {
                ProcessBullet(@params.GetInstance(shootPoints[i]), target);
            }
        }

        /// <summary>
        /// Assign the bullet to the Weapon's owner and increase the owner's signature. 
        /// Then activate the bullet.
        /// </summary>
        /// <param name="bullet">The bullet to be processed.</param>
        /// <param name="target">The target of the bullet.</param>
        public virtual void ProcessBullet(Bullet bullet, IObject target)
        {
            Unit.IncreaseSignature(@params.SignatureIncrease);
            bullet.Owner = Unit;
            bullet.gameObject.SetActive(true);
        }
        #endregion
    }
}