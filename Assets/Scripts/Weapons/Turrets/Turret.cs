using UnityEngine;
using Utilities;
namespace Weapons
{
    public class Turret : MonoBehaviour
    {
        #region Fields
        [Tooltip("The score of the current target of the ship will be multiplied by this " +
            "amount when evaluating targets(divided if negative).")]
        [SerializeField] protected float CurrentTargetScoreMultiplier = 2;
        protected bool ceaseFire;
        public bool CeaseFire
        {
            get => ceaseFire;
            set
            {
                if (ceaseFire != value)
                {
                    ceaseFire = value;
                    if (ceaseFire)
                    {
                        Target = null;
                    }
                }
            }
        }
        public bool TargetMissiles { get; set; }
        protected IObject currentTarget;
        public IObject Target
        {
            get => currentTarget;
            set
            {
                if (currentTarget != value)
                {
                    var m = currentTarget != null ? currentTarget as Projectile : null;
                    if (m != null && m.gameObject.activeSelf) m.TargetedCount--;
                    currentTarget = value;
                    m = currentTarget != null ? currentTarget as Projectile : null;
                    if (m != null && m.gameObject.activeSelf) m.TargetedCount++;
                    if (currentTarget != null && !currentTarget.Active) currentTarget = null;
                }
            }
        }
        public Unit Owner { get; protected set; }
        [SerializeField] protected WeaponBase weapon;
        protected Transform tr;
        [field: SerializeField] public Transform RotationPivot { get; protected set; }
        [field: SerializeField] public Transform ElevationPivot { get; protected set; }
        public Transform Transform => tr != null ? tr : transform;
        public Vector3 Forward => ElevationPivot.forward;
        [SerializeField] protected TurretData @params;
        public float MaxRangeSquared => @params.MaxRangeSquared;
        #endregion

        #region Setup
        protected void Awake()
        {
            tr = transform;
            Owner = GetComponentInParent<Unit>();
            if (weapon == null) weapon = GetComponentInChildren<WeaponBase>();
        }
        protected virtual void OnEnable()
        {
            CeaseFire = false;
            Target = null;
            TargetMissiles = @params.TargetMissiles;
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
        /// <summary>
        /// Returns whether a given position can be targeted by the turret.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsInAngle(Vector3 position)
        {
            var dir = (position - Transform.position).normalized;
            if (Mathf.Abs(Vector3.Dot(Transform.up, dir)) == 1) return false;

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
        public Vector3 ComputeTargetingPosition(IObject target)
        {
            float timeToHit = 0;
            var m = weapon.BulletData as ProjectileData;
            if (m != null) timeToHit = (target.Transform.position -
                    Transform.position).magnitude / m.TopSpeed;

            //Debug.Log(timeToHit);
            var futureTargetPos = target.GetPositionWithOscilation(Owner) +
                target.LinearVelocity * timeToHit;

            return futureTargetPos;
        }

        /// <summary>
        /// Returns whether a given target is valid for the turret to aim at. Checks for 
        /// obstructions, if the target is active, within range, and within the turret's 
        /// angle limits.
        /// </summary>
        /// <param name="target">The target to check.</param>
        /// <returns>True if the target is valid, false otherwise.</returns>
        protected bool IsTargetValid(IObject target)
        {
            if (target == null)
            {
                Debug.LogError("Target is null!");
                return false;
            }
            if (target == null || !target.Transform.gameObject.activeSelf) return false;

            var dir = target.Transform.position - Transform.position;
            if (dir.sqrMagnitude > MaxRangeSquared) return false;
            if (!IsInAngle(target.Transform.position)) return false;
            if (Physics.Linecast(Transform.position, target.Transform.position,
                GlobalConfig.GetTurretObstructionMask(Transform.root.gameObject.layer)))
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
            if (CeaseFire || Owner == null) return;

            //Debug.Log($"Turret {transform.name} checking for targets.");

            if (Target != null && !Target.Transform.gameObject.activeSelf) Target = null;

            float bestScore = Target == null ? float.MinValue : @params.EvaluateTarget(this, Target);

            #region Evaluate missiles
            if (TargetMissiles)
            {
                (var count, var missiles) = Owner.GetMissiles();
                if (count > 0)
                {
                    var currentM = Target != null ? Target as Missile : null;
                    bool targetIsAimedAtOwner = currentM != null && currentM.Target.
                        IsEqual(Owner);

                    for (int i = 0; i < count; i++)
                    {
                        if (missiles[i] == null || !missiles[i].gameObject.activeSelf) continue;
                        var m = missiles[i].transform.GetComponentViaRegister<Missile>();
                        if (m == null || !IsTargetValid(m) || m.TargetedCount > 0) continue;

                        if (Target == null)
                        {
                            targetIsAimedAtOwner = m.Target.IsEqual(Owner);
                            Target = m;
                            bestScore = @params.EvaluateTarget(this, m);
                        }
                        else
                        {
                            if (targetIsAimedAtOwner)
                            {
                                if (m.Target.IsEqual(Owner))
                                {
                                    var newScore = @params.EvaluateTarget(this, m);
                                    if (newScore > bestScore)
                                    {
                                        bestScore = newScore;
                                        Target = m;
                                    }
                                }
                            }
                            else
                            {
                                var newScore = @params.EvaluateTarget(this, m);
                                if (m.Target.IsEqual(Owner))
                                {
                                    bestScore = newScore;
                                    Target = m;
                                    targetIsAimedAtOwner = true;
                                }
                                else if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    Target = m;
                                }
                            }
                        }
                    }



                    if (targetIsAimedAtOwner) return;
                }
            }
            #endregion

            #region Evaluate regular targets
            if (@params.TargetUnits)
            {
                var targets = Owner.GetTargets();
                if (targets.IsEmpty()) return;

                for (int i = 0; i < targets.Count; i++)
                {
                    if (!IsTargetValid(targets[i])) continue;
                    //Debug.Log("Evaluating target: " + targets[i].Transform.name);
                    var score = @params.EvaluateTarget(this, targets[i]);
                    //Debug.Log($"Target {targets[i].Transform.name} scored {score}.");
                    if (score <= 0 && !@params.AllowZeroScoreTarget) continue;
                    if (Owner.Target != null && targets[i].GetInstanceID() ==
                        Owner.Target.GetInstanceID())
                    {
                        if (score >= 0) score *= CurrentTargetScoreMultiplier;
                        else score /= CurrentTargetScoreMultiplier;
                    }
                    if (score > bestScore + @params.MinScoreDelta)
                    {
                        bestScore = score;
                        Target = targets[i];
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// If a target is present and enabled, rotate towards the target, 
        /// shoot and increase readiness. Otherwise decrease readiness and 
        /// call UpdateTargets().
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Tick(float deltaTime)
        {
            if (Target != null)
            {
                if (Target.Transform.gameObject.activeSelf)
                {
                    //Debug.Log($"Turret {transform.name} targeting {Target.Transform.name}.");
                    var pos = ComputeTargetingPosition(Target);
                    Debug.DrawLine(Transform.position, pos, Color.red);
                    LookAt(pos, deltaTime);
                    if ((Transform.position - pos).sqrMagnitude <= @params.MaxRangeSquared)
                    {
                        //Debug.Log($"Turret {transform.name} is aiming at {Target.Transform.name}.");
                        if (Vector3.Angle(ElevationPivot.forward, (pos -
                            Transform.position)) < @params.MaxAngleDifference)
                        {
                            IncreaseReadiness(deltaTime);
                            weapon.Shoot(Target);
                            return;
                        }
                    }
                }
                else Target = null;
            }
            else
            {
                RotateToIdle(deltaTime);
                UpdateTargets();
            }
            DecreaseReadiness(deltaTime);
        }

        /// <summary>
        /// Increases readiness on the weapon.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected void IncreaseReadiness(float deltaTime)
        {
            weapon.IncreaseReadiness(deltaTime);
        }

        /// <summary>
        /// Calls DecreaseReadiness on the weapon.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected void DecreaseReadiness(float deltaTime)
        {
            weapon.DecreaseReadiness(deltaTime);
        }

        /// <summary>
        /// Rotate the turret to idle rotation.
        /// </summary>
        /// <param name="deltaTime"></param>
        protected void RotateToIdle(float deltaTime)
        {
            //Debug.Log("Rotating turret to idle position.");
            if (RotationPivot.rotation == Quaternion.identity && ElevationPivot.rotation == Quaternion.identity) return;

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
    }
}