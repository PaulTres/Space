using UnityEngine;
using Utilities;
namespace Weapons
{
    public class Turret : MonoBehaviour
    {
        protected bool ceaseFire;
        public bool CeaseFire
        {
            get => ceaseFire;
            set
            {
                if (ceaseFire != value)
                {
                    ceaseFire = value;
                    if (!ceaseFire)
                    {
                        Target = null;
                    }
                }
            }
        }
        public bool TargetMissiles { get; set; }
        public IObject Target { get; protected set; }
        public Unit Owner { get; protected set; }
        [SerializeField] protected WeaponBase weapon;
        protected Transform tr;
        [field: SerializeField] public Transform RotationPivot { get; protected set; }
        [field: SerializeField] public Transform ElevationPivot { get; protected set; }
        public Transform Transform => tr != null ? tr : transform;
        public Vector3 Forward => ElevationPivot.forward;
        [SerializeField] protected TurretData @params;
        public float MaxRangeSquared => @params.MaxRangeSquared;

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
            GlobalUpdater.TryGetInstance().UnregisterUpdate(Tick);
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

            var localRotation = Quaternion.Inverse(Transform.rotation) *
                Quaternion.LookRotation(dir, Transform.up);

            if (localRotation.eulerAngles.y < @params.MinRot ||
                localRotation.eulerAngles.y > @params.MaxRot) return false;

            if (localRotation.eulerAngles.x < -@params.MaxElevation ||
                localRotation.eulerAngles.x > -@params.MinElevation) return false;

            return true;
        }

        public Vector3 ComputeTargetingPosition(IObject target)
        {
            float timeToHit = 0;
            var m = weapon.BulletData as ProjectileData;
            if (m != null) timeToHit = (target.Transform.position -
                    Transform.position).magnitude / m.TopSpeed;

            var futureTargetPos = target.GetPositionWithOscilation(Owner) +
                target.LinearVelocity * timeToHit;

            return futureTargetPos;
        }

        protected bool IsTargetValid(IObject target)
        {
            if (target == null) return false;
            var dir = target.Transform.position - Transform.position;
            if (dir.sqrMagnitude > MaxRangeSquared) return false;
            if (!IsInAngle(target.Transform.position)) return false;
            return true;
        }

        public void UpdateTargets()
        {
            if (CeaseFire || Owner == null) return;

            if (Target != null && !Target.Transform.gameObject.activeSelf) Target = null;

            float bestScore = Target == null ? 0 : @params.EvaluateTarget(this, Target);

            #region Evaluate missiles
            if (TargetMissiles)
            {
                (var count, var missiles) = Owner.GetMissiles();
                if (count > 0)
                {
                    var currentM = Target as Missile;
                    bool targetIsAimedAtOwner = currentM != null && currentM.Target.
                        IsEqual(Owner);


                    for (int i = 0; i < count; i++)
                    {
                        if (missiles[i] == null) continue;
                        var m = missiles[i].transform.GetComponentViaRegister<Missile>();
                        if (m == null || !IsTargetValid(m)) continue;

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

                    var score = @params.EvaluateTarget(this, targets[i]);
                    if (score <= 0) return;

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
                    var pos = ComputeTargetingPosition(Target);
                    if ((Transform.position - pos).sqrMagnitude <= @params.MaxRangeSquared)
                    {
                        LookAt(pos, deltaTime);
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
            DecreaseReadiness(deltaTime);
            UpdateTargets();
        }

        protected void IncreaseReadiness(float deltaTime)
        {
            weapon.IncreaseReadiness(deltaTime);
        }

        protected void DecreaseReadiness(float deltaTime)
        {
            weapon.DecreaseReadiness(deltaTime);
            RotateToIdle(deltaTime);
        }

        protected void RotateToIdle(float deltaTime)
        {
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
        }
        #endregion
    }
}