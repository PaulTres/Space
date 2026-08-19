using System.Collections.Generic;
using UnityEngine;
using Utilities;
namespace Weapons
{
    public class Turret : MonoBehaviour
    {
        #region Fields
        protected ImprovedMonoBehaviour currentTarget;
        public ImprovedMonoBehaviour Target
        {
            get => currentTarget.Active() ? currentTarget : null;
            set
            {
                if (currentTarget != value)
                {
                    var m = currentTarget != null ? currentTarget as Missile : null;
                    if (m.Active()) m.TargetedDamage -= DPS;
                    currentTarget = value;
                    m = currentTarget != null ? currentTarget as Missile : null;
                    if (m.Active()) m.TargetedDamage += DPS;
                }
            }
        }
        public float DPS { get; protected set; }
        public float BulletSpeed => Weapons.IsEmpty() ? 0 : Weapons[0].BulletSpeed;
        public float SignatureIncrease => Weapons.IsEmpty() ? 0 : Weapons[0].SignatureIncrease * Weapons.Count;
        /// <summary>
        /// The unit owning this turret.
        /// </summary>
        public Unit Unit { get; protected set; }
        public List<WeaponBase> Weapons { get; protected set; }
        protected Transform tr;
        [field: SerializeField] public Transform RotationPivot { get; protected set; }
        [field: SerializeField] public Transform ElevationPivot { get; protected set; }
        public Transform Transform => tr != null ? tr : transform;
        public Vector3 Forward => ElevationPivot.forward;
        [SerializeField] protected TurretData @params;
        public TurretData Parameters => @params;
        public bool TargetMissiles { get; set; }
        public bool TargetUnits { get; set; }
        public float MaxRangeSquared => @params.MaxRangeSquared;
        /// <summary>
        /// Stance 0 - aggressive<br/>
        /// Stance 1 - defensive<br/>
        /// Stance 2 - passive
        /// </summary>
        protected List<TurretStance> turretStances = new();
        public int CurrentStanceIndex { get; protected set; } = 0;
        protected float lastBurstTime;
        protected int weaponBurstIndex;
        /// <summary>
        /// 0 - fully reloaded<br/>
        /// 1 - just fired<br/>
        /// </summary>
        public float ReloadStatus
        {
            get
            {
                float s = 0;
                for (int i = 0; i < Weapons.Count; i++) s += Weapons[i].ReloadStatus;
                return s / Weapons.Count;
            }
        }
        #endregion

        #region Setup
        protected void Awake()
        {
            Unit = GetComponentInParent<Unit>();
            if (RotationPivot == null) RotationPivot = Transform;
            if (ElevationPivot == null) ElevationPivot = Transform;
            Weapons = new(GetComponentsInChildren<WeaponBase>());
            turretStances.Add(new AggressiveTurretStance() { Turret = this, TurretData = @params });
            turretStances.Add(new DefensiveTurretStance() { Turret = this, TurretData = @params });
            turretStances.Add(new PassiveTurretStance() { Turret = this, TurretData = @params });
        }

        protected virtual void OnEnable()
        {
            TargetMissiles = @params.TargetMissiles;
            TargetUnits = @params.TargetUnits;
            lastBurstTime = float.MinValue;
            weaponBurstIndex = 0;
            RecalculateDPS();
            Target = null;
            CurrentStanceIndex = 0;
            GlobalUpdater.TryGetInstance(true).RegisterUpdate(Tick);
        }

        protected virtual void OnDisable()
        {
            Target = null;
            var u = GlobalUpdater.TryGetInstance();
            if (u != null) u.UnregisterUpdate(Tick);
        }
        #endregion

        #region Functionality
        public void Shoot()
        {
            if (@params.BurstFire)
            {
                if (Time.time - lastBurstTime >= @params.BurstCooldown && Weapons[weaponBurstIndex].CanShoot())
                {
                    //Debug.Log("Burst firing " + weaponBurstIndex + " " + Weapons[weaponBurstIndex].CanShoot());
                    lastBurstTime = Time.time;
                    int i;
                    for (i = weaponBurstIndex; i < Mathf.Min(Weapons.Count, weaponBurstIndex + @params.BurstCount); i++)
                    {
                        //Debug.Log($"Turret {transform.name} firing weapon {i} at target {Target.Transform.name}.");
                        Weapons[i].Shoot(Target);
                    }
                    if (i > Weapons.Count - 1) weaponBurstIndex = 0;
                    else weaponBurstIndex = i;
                }
            }
            else
            {
                foreach (var w in Weapons)
                {
                    //if (Target == GameManager.Player) Debug.Log(w.transform);
                    w.Shoot(Target);
                }
            }
        }

        public void RecalculateDPS()
        {
            if (@params.BurstFire)
            {
                DPS = Mathf.Min(@params.BurstCount, Weapons.Count) * Weapons[0].DPS / @params.BurstCooldown;
            }
            else DPS = Weapons[0].DPS * Weapons.Count;
        }

        /// <summary>
        /// If a target is present and enabled, rotate towards the target, 
        /// shoot and increase readiness. Otherwise decrease readiness and 
        /// call UpdateTargets().
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Tick(float deltaTime)
        {
            if (!Unit.Active()) return;
            if (Target.Active())
            {
                //if (Target == GameManager.Player) Debug.Log($"Turret {transform.name} targeting {Target.Transform}.");
                //if (@params.BurstFire) Debug.Log($"Turret {transform.name} targeting {Target.Transform.name}.");
                var pos = ComputeTargetingPosition(Target);
                //Debug.DrawLine(Transform.position, pos, Color.red);
                LookAt(pos, deltaTime);
                if ((Transform.position - pos).sqrMagnitude <= @params.MaxRangeSquared)
                {
                    //if (Target == GameManager.Player && !@params.TargetMissiles) Debug.Log($"Turret {transform.name} is aiming at {Target.Transform.name}.");
                    if (@params.MaxAngleDifference >= 360 || Vector3.Angle(ElevationPivot.forward, (pos - Transform.position)) < @params.MaxAngleDifference)
                    {
                        //if (!@params.TargetMissiles && Target == GameManager.Player) Debug.Log($"Turret {transform.name} is aiming at {Target.Transform.name}.");
                        Shoot();
                        IncreaseReadiness(deltaTime);
                        return;
                    }
                }
            }
            else
            {
                RotateToIdle(deltaTime);
                UpdateTargets();
            }
            DecreaseReadiness(deltaTime);
        }

        /// <summary>
        /// Increases readiness on the Weapons.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected void IncreaseReadiness(float deltaTime)
        {
            foreach (var w in Weapons) w.IncreaseReadiness(deltaTime);
        }

        /// <summary>
        /// Calls DecreaseReadiness on the Weapons.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected void DecreaseReadiness(float deltaTime)
        {
            foreach (var w in Weapons) w.DecreaseReadiness(deltaTime);
        }

        #region Targeting
        #region Stance
        /// <summary>
        /// Sets stance index to index%(nr of stances), then calls UpdateTargets.
        /// </summary>
        /// <param name="index"></param>
        public void SetStanceIndex(int index)
        {
            CurrentStanceIndex = index % turretStances.Count;
            UpdateTargets();
        }

        /// <summary>
        /// Increases stance index by 1, loops around on reaching end of stance list. Then calls UpdateTargets.
        /// </summary>
        public void CycleStance() => SetStanceIndex(CurrentStanceIndex + 1);

        public void SetAggressiveStance() => SetStanceIndex(0);

        public void SetDefensiveStance() => SetStanceIndex(1);

        public void SetPassiveStance() => SetStanceIndex(2);
        #endregion

        /// <summary>
        /// Returns whether a given position can be targeted by the turret.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsInAngle(Vector3 position)
        {
            if (@params.MaxAngleDifference >= 360) return true;
            var dir = (position - Transform.position).normalized;
            if (dir.sqrMagnitude == 0) return false;
            if (Mathf.Abs(Vector3.Dot(Transform.up, dir)) == 1)
            {
                return @params.MaxAngleDifference >= 90 || @params.MaxElevation >= 90;
            }

            var localRotation = Quaternion.Inverse(Transform.rotation) *
                Quaternion.LookRotation(dir, Transform.up);

            var y = Extensions.NormalizeAngle(localRotation.eulerAngles.y);
            if (y < @params.MinRot || y > @params.MaxRot) return false;

            var x = Extensions.NormalizeAngle(localRotation.eulerAngles.x);
            if (x < -@params.MaxElevation || x > -@params.MinElevation) return false;

            return true;
        }

        /// <summary>
        /// Computes the position to aim at for a given target, taking into account the 
        /// target's movement and the projectile speed. May introduce randomness based on 
        /// the target's signature.
        /// </summary>
        /// <param name="target">The target to aim at.</param>
        /// <returns>The position to aim at.</returns>
        public Vector3 ComputeTargetingPosition(ImprovedMonoBehaviour target)
        {
            float timeToHit = 0;
            var m = Weapons[0].BulletData as ProjectileData;
            if (m != null) timeToHit = (target.Transform.position -
                    Transform.position).magnitude / (m.Speed + Mathf.Epsilon);

            //Debug.Log(timeToHit);
            var futureTargetPos = target.GetPositionWithOscilation(Unit) +
                target.LinearVelocity * timeToHit;

            return futureTargetPos;
        }

        /// <summary>
        /// Returns whether a given target is valid for the turret to aim at. 
        /// Checks for obstructions, if the target is active, within range, 
        /// and within the turret's angle limits.
        /// </summary>
        /// <param name="target">The target to check.</param>
        /// <returns>True if the target is valid, false otherwise.</returns>
        public bool IsTargetValid(ImprovedMonoBehaviour target)
        {
            if (!target.Active()) return false;
            if (Weapons[0].RequiresOptimalFiringRange && !Unit.InOptimalFiringRange(target)) return false;

            //if (@params.BurstFire) Debug.Log(target.Transform);
            var dir = target.Transform.position - Transform.position;
            if (dir.sqrMagnitude > MaxRangeSquared) return false;
            if (!IsInAngle(target.Transform.position)) return false;
            if (@params.AccountForObstructions && Physics.Linecast(
                Transform.position, target.Transform.position,
                GlobalConfig.GetTurretObstructionMask(Unit.Team)))
            {
                Debug.Log("obstructed");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Go over available targets and select the best one based on the turret's 
        /// parameters. If a target is already selected, it will only be replaced if 
        /// a better one is found.
        /// </summary>
        public void UpdateTargets()
        {
            if (!Unit.Active()) return;
            turretStances[CurrentStanceIndex].UpdateTargets();
        }
        #endregion

        #region Turret movement
        /// <summary>
        /// Rotate the turret to idle rotation.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected void RotateToIdle(float deltaTime)
        {
            //Debug.Log("Rotating turret to idle position.");
            if (@params.RotSpeed == 0 || (RotationPivot.rotation == Quaternion.identity && ElevationPivot.rotation == Quaternion.identity)) return;

            if (RotationPivot == ElevationPivot)
            {
                var targetRot = Quaternion.RotateTowards(RotationPivot.rotation,
                    Transform.rotation, deltaTime * @params.RotSpeed);
                RotationPivot.rotation = targetRot;
            }
            else
            {
                var horizTargetRot = Quaternion.RotateTowards(RotationPivot.rotation,
                   Transform.rotation, deltaTime * @params.RotSpeed);
                RotationPivot.rotation = horizTargetRot;

                var elevTargetRot = Quaternion.RotateTowards(ElevationPivot.rotation,
                    Transform.rotation, deltaTime * @params.RotSpeed);
                ElevationPivot.rotation = elevTargetRot;
            }
        }

        /// <summary>
        /// Rotate towards a given position. Performs no checks to see if that is possible.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="deltaTime"></param>
        protected void LookAt(Vector3 position, float deltaTime)
        {
            if (@params.RotSpeed == 0) return;
            Vector3 direction = position - Transform.position;
            if (direction.sqrMagnitude > 0.01)
            {
                if (RotationPivot == ElevationPivot)
                {
                    RotateUnified(direction.normalized, deltaTime);
                }
                else RotateSeparate(direction.normalized, deltaTime);
            }
        }

        /// <summary>
        /// Treats the rotation and elevation pivots as if they are the same object. Rotates 
        /// them to face a given target.
        /// </summary>
        /// <param name="direction">The direction to rotate towards.</param>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        protected void RotateUnified(Vector3 direction, float deltaTime)
        {
            var newRotation = Quaternion.LookRotation(direction, Transform.up);
            var targetRot = Quaternion.RotateTowards(RotationPivot.rotation,
                newRotation, deltaTime * @params.RotSpeed);

            RotationPivot.rotation = targetRot;
            var targetYaw = Mathf.Clamp(Extensions.NormalizeAngle(RotationPivot.
                localRotation.eulerAngles.y), @params.MinRot, @params.MaxRot);
            var targetPitch = Mathf.Clamp(Extensions.NormalizeAngle(RotationPivot.
                localRotation.eulerAngles.x), -@params.MaxElevation, -@params.MinElevation);

            RotationPivot.localRotation = Quaternion.Euler(targetPitch, targetYaw, 0);
        }

        /// <summary>
        /// Rotate the rotation pivot and elevation pivot separately towards a given 
        /// direction. Performs no checks to see if that is possible.
        /// </summary>
        /// <param name="direction">The direction to rotate towards.</param>
        /// <param name="deltaTime">The time elapsed since the last update.</param>
        protected void RotateSeparate(Vector3 direction, float deltaTime)
        {
            var newRotation = Quaternion.LookRotation(direction, Transform.up);

            var horizTargetRot = Quaternion.RotateTowards(RotationPivot.rotation,
                newRotation, deltaTime * @params.RotSpeed);
            RotationPivot.rotation = horizTargetRot;
            var targetYaw = Mathf.Clamp(Extensions.NormalizeAngle(RotationPivot.
                localRotation.eulerAngles.y), @params.MinRot, @params.MaxRot);
            RotationPivot.localRotation = Quaternion.Euler(0, targetYaw, 0);

            //newRotation = Quaternion.LookRotation(direction, Transform.up);
            var elevTargetRot = Quaternion.RotateTowards(ElevationPivot.rotation,
                newRotation, deltaTime * @params.RotSpeed);
            ElevationPivot.rotation = elevTargetRot;
            var targetPitch = Mathf.Clamp(Extensions.NormalizeAngle(ElevationPivot.
                localRotation.eulerAngles.x), -@params.MaxElevation, -@params.MinElevation);
            //var targetPitch=Mathf.Clamp(ElevationPivot.localRotation.x,)
            ElevationPivot.localRotation = Quaternion.Euler(targetPitch, 0, 0);

            //Debug.Log($"Turret {transform.name} rotated to yaw {targetYaw} and pitch {targetPitch}: {ElevationPivot.localRotation.eulerAngles}.");
        }
        #endregion
        #endregion
    }
}