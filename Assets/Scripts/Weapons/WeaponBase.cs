using Timers;
using UnityEngine;
using Utilities;
namespace Weapons
{
    public abstract class WeaponBase : MonoBehaviour, IResettable
    {
        #region Utils   
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
        public float CurrentShotCooldown { get; protected set; }
        public BulletData BulletData => @params != null ? @params.BulletData : null;
        public float BulletSpeed => @params.BulletData != null ? @params.BulletData.Speed : 0;
        public float DPS => @params.BulletData.Damage * shootPoints.Length / CurrentShotCooldown;
        public float SignatureIncrease => @params.SignatureIncrease;
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
        [SerializeField] protected Renderer[] readinessDisplays;
        [SerializeField] protected Color noReadinessColor = Color.black, fullReadinessColor = Color.green;
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
            shotTimer = new(@params.MaxShotCooldown);
            CurrentShotCooldown = @params.MaxShotCooldown;
            if (!readinessDisplays.IsEmpty())
            {
                foreach (var display in readinessDisplays)
                {
                    display.material.EnableKeyword("_EMISSION");
                }
            }
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
        #region Display
        public void UpdateReadinessRenderers()
        {
            if (readinessDisplays.IsEmpty()) return;
            //we need 0 when CurrentShotCooldown==@params.MaxShotCooldown and 1 when CurrentShotCooldown==@params.MinShotCooldown
            float lerpValue = (@params.MaxShotCooldown - CurrentShotCooldown) / (@params.MaxShotCooldown - @params.MinShotCooldown);
            var newColor = Color.Lerp(noReadinessColor, fullReadinessColor, lerpValue);
            //Debug.Log(lerpValue);
            for (int i = 0; i < readinessDisplays.Length; i++)
            {
                readinessDisplays[i].material.SetColor("_EmissionColor", newColor);
            }
        }
        #endregion

        /// <summary>
        /// Increases weapon rate of fire.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void IncreaseReadiness(float deltaTime)
        {
            //Debug.Log("Increasing readiness: " + CurrentShotCooldown);
            CurrentShotCooldown = Mathf.Max(@params.MinShotCooldown,
                CurrentShotCooldown - @params.RateOfFireRampUp * deltaTime);
            UpdateReadinessRenderers();
        }

        /// <summary>
        /// Decreases weapon rate of fire.
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void DecreaseReadiness(float deltaTime)
        {
            //Debug.Log("Decreasing readiness: " + CurrentShotCooldown);
            CurrentShotCooldown = Mathf.Min(@params.MaxShotCooldown,
                CurrentShotCooldown + @params.RateOfFireRampDown * deltaTime);
            UpdateReadinessRenderers();
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
            shotTimer.Reset(CurrentShotCooldown);
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
                ProcessBullet(@params.GetInstance(shootPoints[i]), target, shootPoints[i]);
            }
        }

        /// <summary>
        /// Assign the bullet to the Weapons's owner and increase the owner's signature. 
        /// Then activate the bullet.
        /// </summary>
        /// <param name="bullet">The bullet to be processed.</param>
        /// <param name="target">The target of the bullet.</param>
        public virtual void ProcessBullet(Bullet bullet, ImprovedMonoBehaviour target, Transform shootPoint)
        {
            UnitProperty.IncreaseSignature(@params.SignatureIncrease);
            bullet.Owner = UnitProperty;
            bullet.gameObject.SetActive(true);
        }
        #endregion
        #endregion
    }
}