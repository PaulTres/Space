using Utilities;

namespace Weapons
{
    /// <summary>
    /// Governs turret target selection.
    /// </summary>
    public abstract class TurretStance
    {
        public Turret Turret;
        public TurretData TurretData;

        public ImprovedMonoBehaviour Target
        {
            get => Turret.Target;
            set => Turret.Target = value;
        }
        public Unit Owner => Turret.Unit;

        public bool IsTargetValid(ImprovedMonoBehaviour @object) => Turret.IsTargetValid(@object);

        public float EvaluateTarget(ImprovedMonoBehaviour @object) => TurretData.EvaluateTarget(Turret, @object);

        public abstract void UpdateTargets();
    }

    /// <summary>
    /// Default turret stance. The turret will engage any available target.
    /// </summary>
    public class AggressiveTurretStance : TurretStance
    {
        public override void UpdateTargets()
        {
            float bestScore = Target.Active() ? EvaluateTarget(Target) : float.MinValue;

            #region Evaluate missiles
            if (Turret.TargetMissiles)
            {
                (var count, var missiles) = Owner.GetMissiles();
                //Debug.Log(count);
                if (count > 0)
                {
                    var currentM = Target.Active() ? Target as Missile : null;
                    bool targetIsAimedAtOwner = currentM != null && currentM.Target.
                        IsEqual(Owner);

                    for (int i = 0; i < count; i++)
                    {
                        if (missiles[i] == null || !missiles[i].gameObject.activeSelf) continue;
                        var m = missiles[i].transform.GetComponentViaRegister<Missile>();
                        if (m == null || !IsTargetValid(m) || m.TargetedDamage >= m.MaxHP) continue;

                        if (Target == null)
                        {
                            targetIsAimedAtOwner = m.Target.IsEqual(Owner);
                            Target = m;
                            bestScore = EvaluateTarget(m);
                        }
                        else
                        {
                            if (targetIsAimedAtOwner)
                            {
                                if (m.Target.IsEqual(Owner))
                                {
                                    var newScore = EvaluateTarget(m);
                                    if (newScore > bestScore)
                                    {
                                        bestScore = newScore;
                                        Target = m;
                                    }
                                }
                            }
                            else
                            {
                                var newScore = EvaluateTarget(m);
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
            if (Turret.TargetUnits)
            {

                var targets = Owner.GetTargets();
                if (targets.IsEmpty()) return;

                for (int i = 0; i < targets.Count; i++)
                {
                    if (!IsTargetValid(targets[i])) continue;
                    //if (TurretData.BurstFire) Debug.Log("Evaluating target: " + targets[i].Transform.name);
                    var score = EvaluateTarget(targets[i]);
                    //if (Turret.Unit == GameManager.Player && !Turret.TargetMissiles && Unit.Target.Active())

                    //if (TurretData.BurstFire) Debug.Log($"Target {targets[i].Transform.name} scored {score}.");
                    if (score <= 0 && !TurretData.AllowZeroScoreTargets) continue;

                    if (Owner.Target.Active() && targets[i].Transform.root.GetInstanceID() ==
                        Owner.Target.Transform.root.GetInstanceID())
                    {
                        if (score >= 0) score *= TurretData.CurrentTargetScoreMultiplier;
                        else score /= TurretData.CurrentTargetScoreMultiplier;
                    }
                    if (score > bestScore + TurretData.MinScoreDelta)
                    {
                        bestScore = score;
                        Target = targets[i];
                    }
                }
            }
            #endregion
        }
    }

    /// <summary>
    /// The turret will only shoot at the current target or at missiles 
    /// targeting the ship.
    /// </summary>
    public class DefensiveTurretStance : TurretStance
    {
        public override void UpdateTargets()
        {
            if (Turret.TargetMissiles)
            {
                (var count, var missiles) = Owner.GetMissiles();
                //Debug.Log(count);
                if (count > 0)
                {
                    var currentM = Target.Active() ? Target as Missile : null;
                    bool targetIsAimedAtOwner = currentM != null && currentM.Target.IsEqual(Owner);
                    if (!targetIsAimedAtOwner && currentM != null) Target = null;
                    float bestScore = EvaluateTarget(Target);

                    for (int i = 0; i < count; i++)
                    {
                        if (missiles[i] == null || !missiles[i].gameObject.activeSelf) continue;
                        var m = missiles[i].transform.GetComponentViaRegister<Missile>();
                        if (m == null || !IsTargetValid(m) || m.TargetedDamage >= m.MaxHP) continue;

                        if (m.Target.IsEqual(Owner))
                        {
                            if (Target == null)
                            {
                                Target = m;
                                targetIsAimedAtOwner = true;
                                bestScore = EvaluateTarget(m);
                            }
                            else
                            {
                                var score = EvaluateTarget(m);
                                if (score > bestScore)
                                {
                                    bestScore = score;
                                    Target = m;
                                }
                            }

                        }
                    }

                    if (targetIsAimedAtOwner) return;
                }
            }

            if (Turret.TargetUnits)
            {
                Target = Owner.Target;
            }
        }
    }

    /// <summary>
    /// The turret will not shoot at anything no matter what.
    /// </summary>
    public class PassiveTurretStance : TurretStance
    {
        public override void UpdateTargets()
        {
            Target = null;
        }
    }
}