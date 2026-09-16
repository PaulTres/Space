using Timers;
using UnityEngine;
using Utilities;
namespace Weapons
{
    public abstract class WeaponBase : MonoBehaviour, IResettable
    {
        #region Helpers
        Unit unit;
        /// <summary>
        /// The unit that owns this Weapons. Lazily initialized by searching for a Navigation 
        /// component in the root of the transform hierarchy.
        /// </summary>
        public Unit UnitProperty
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
        protected float readinessValue;
        public float ReadinessValue
        {
            get => readinessValue;
            set
            {
                if (readinessValue != value)
                {
                    readinessValue = value;
                    currentShotCooldown = Mathf.Lerp(@params.MinShotCooldown, @params.MaxShotCooldown, @params.GetCurveValue(readinessValue));
                }
            }
        }

        public BulletData BulletData => @params != null ? @params.BulletData : null;
        public float BulletSpeed => @params.BulletData != null ? @params.BulletData.Speed : 0;
        public float DPS => @params.BulletData.Damage * shootPoints.Length / currentShotCooldown;
        public float SignatureIncreaseRaw => @params.SignatureIncrease * shootPoints.Length;
        /// <summary>
        /// Gives the signature increase divided by the shot cooldown.
        /// </summary>
        public float SignatureIncrease => (@params.SignatureIncrease * shootPoints.Length) / currentShotCooldown;
        /// <summary>
        /// 0 means the weapon is fully reloaded, 1 means the weapon just fired and has started reloading.
        /// </summary>
        public float ReloadStatus
        {
            get
            {
                if (shotTimer.IsRunning) return shotTimer.Progress;
                return 0;
            }
        }
        public bool RequiresOptimalFiringRange => @params.RequiresOptimalFiringRange;
        #endregion

        #region Parameters
        [SerializeField] protected WeaponData @params;
        [Tooltip("A separate bullet will be spawned with the position and rotation of each of these objects.")]
        [SerializeField] protected Transform[] shootPoints;
        #endregion

        #region Setup
        protected virtual void Awake()
        {
            Debug.Assert(@params != null, $"{transform} has no Weapons parameters!");
            Debug.Assert(@params.MaxShotCooldown >= 0, $"{transform} has negative cooldown!");
            if (shootPoints.IsEmpty())
            {
                //Debug.LogWarning($"{transform} has no shoot points!");
                shootPoints = new Transform[] { transform };
            }
            //currentShotCooldown = @params.MaxShotCooldown;
            ReadinessValue = 0;
            shotTimer ??= new(currentShotCooldown);
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
            shotTimer ??= new CountdownTimer(@params.MaxShotCooldown);
            //currentShotCooldown = @params.MaxShotCooldown;
            ReadinessValue = 0;
            shotTimer.Reset(currentShotCooldown);
            if (!@params.StartLoaded) shotTimer.Start();
        }

        protected virtual void OnDisable()
        {
            shotTimer?.Stop();
        }

        protected virtual void OnDestroy()
        {
            shotTimer?.Dispose();
        }
        #endregion

        #region Functionality

        /// <summary>
        /// Increases weapon rate of fire.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void IncreaseReadiness(float deltaTime)
        {
            if (@params.RateOfFireRampUp > 0)
            {
                ReadinessValue = Mathf.Min(1, ReadinessValue + deltaTime * @params.RateOfFireRampUp);
            }
        }

        /// <summary>
        /// Decreases weapon rate of fire.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void DecreaseReadiness(float deltaTime)
        {
            if (@params.RateOfFireRampDown > 0)
            {
                ReadinessValue = Mathf.Max(0, ReadinessValue - deltaTime * @params.RateOfFireRampDown);
            }
        }

        /// <summary>
        /// Checks if the weapon can shoot by using the shot cooldown timer.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanShoot()
        {
            return shotTimer == null || !shotTimer.IsRunning;
        }

        #region Shooting
        /// <summary>
        /// Returns true if the Weapons can shoot and fires a bullet at the target. 
        /// Resets the shot cooldown timer.
        /// </summary>
        /// <param name="target">The target to shoot at.</param>
        /// <returns>True if the Weapons successfully fired a shot, false otherwise.</returns>
        public bool Shoot(ImprovedMonoBehaviour target)
        {
            if (!CanShoot()) return false;
            if (@params.RequiresOptimalFiringRange && !UnitProperty.InOptimalFiringRange(target)) return false;
            Fire(target);
            shotTimer.Reset(currentShotCooldown);
            shotTimer.Start();
            return true;
        }

        /// <summary>
        /// Fire once for every shoot point.
        /// </summary>
        public virtual void Fire(ImprovedMonoBehaviour target)
        {
            for (int i = 0; i < shootPoints.Length; i++)
            {
                if (shootPoints[i] == null || !shootPoints[i].gameObject.activeSelf) continue;
                ProcessBullet(@params.GetInstance(shootPoints[i]), target);
            }
        }

        /// <summary>
        /// Assign the bullet to the Weapons's owner and increase the owner's signature. 
        /// Then activate the bullet.
        /// </summary>
        /// <param name="bullet">The bullet to be processed.</param>
        /// <param name="target">The target of the bullet.</param>
        public void ProcessBullet(Bullet bullet, ImprovedMonoBehaviour target)
        {
            UnitProperty.IncreaseSignature(@params.SignatureIncrease);
            bullet.Owner = UnitProperty;
            PerformBulletInitialization(bullet, target);
            bullet.gameObject.SetActive(true);
        }

        /// <summary>
        /// Initializes the bullet with extra information, such as its target.
        /// </summary>
        /// <param name="bullet"></param>
        /// <param name="target"></param>
        protected abstract void PerformBulletInitialization(Bullet bullet, ImprovedMonoBehaviour target);
        #endregion
        #endregion
    }
}