using HybridBT2;
using UnityEngine;
using Utilities;

public class Navigation : Ship
{
    #region Fields
    protected Collider[] buffer = new Collider[10];
    public Vector3? Destination { get; protected set; }
    public bool HasDestination => Destination != null;
    [Header("Parameters")]
    public Transform NavPivot { get; set; }
    [field: SerializeField] public float StopDistance { get; set; } = 0.5f;
    public float angleToForward = 10;
    protected float radAngleToForward;
    [SerializeField] protected ObstacleChecker checker;
    public bool UpdateRotation => RotationTarget == null;
    /// <summary>
    /// Describes what rotation this object should have. NULL means the object 
    /// will face its velocity.
    /// </summary>
    public RotationTarget RotationTarget { get; set; }
    public Vector3 IntendedForward
    {
        get
        {
            if (RotationTarget == null) return Transform.forward;
            return RotationTarget.GetTargetRotation(Transform) * Vector3.forward;
        }
    }
    /// <summary>
    /// The unit this is currently targeting.
    /// </summary>
    public override ImprovedMonoBehaviour Target
    {
        get
        {
            var blackboard = Transform.GetBlackboard();
            if (blackboard == null) return null;
            return blackboard.GetData<ImprovedMonoBehaviour>(Blackboard.Keys.Target);
        }
    }
    #endregion

    #region Setup
    protected override void Awake()
    {
        base.Awake();
        if (NavPivot == null)
        {
            NavPivot = new GameObject("Auto-generated nav pivot").transform;
            NavPivot.SetPositionAndRotation(transform.position, transform.rotation);
            NavPivot.parent = transform;
            //Debug.LogWarning($"Created new nav pivot for {transform}.");
        }
        radAngleToForward = Mathf.Rad2Deg * angleToForward / 2;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GlobalUpdater.TryGetInstance(true).RegisterUpdate(PerformUpdate);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        buffer = null;
        RotationTarget = null;
        if (GlobalUpdater.TryGetInstance() != null) GlobalUpdater.TryGetInstance().UnregisterUpdate(PerformUpdate);
    }
    #endregion

    public void SetDestination(Vector3? destination) => Destination = destination;

    protected void PerformUpdate(float deltaTime)
    {
        if (Destination == null)
        {
            if (Dampeners) ApplyDampeners(Vector3.zero, deltaTime);
            return;
        }
        NavPivot.LookAt(Destination.Value);
        Vector3 direction = Destination.Value - Transform.position;
        if (direction.magnitude <= StopDistance || Vector3.Distance(transform.position,
            Destination.Value) <= StopDistance)
        {
            Destination = null;
            return;
        }
        direction = direction.normalized;
        if (!checker.CheckForward(this, direction, buffer))
        {
            MoveTowards(direction, deltaTime);
        }
        else
        {
            //Debug.Log("A");
            var point = checker.RunCheck(this, buffer);
            MoveTowards(point, deltaTime);
        }
    }

    public void Face(Quaternion targetRotation, float deltaTime)
    {
        Quaternion previousRotation = transform.rotation;
        transform.rotation = Quaternion.RotateTowards(previousRotation, targetRotation,
            RotationSpeed * deltaTime);

        Quaternion rotationDelta = (transform.rotation *
            Quaternion.Inverse(previousRotation)).normalized;
        rotationDelta.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        if (angleInDegrees > 180f) angleInDegrees -= 360f;

        angularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / deltaTime;
    }

    public void Face(Vector3 direction, float deltaTime)
    {
        if (direction == Vector3.zero) return;
        Face(Quaternion.LookRotation(direction, direction + transform.up), deltaTime);
    }

    /// <summary>
    /// Slowly adjust the rigidbody's linear velocity towards the intended velocity, 
    /// simulating a dampening effect.
    /// </summary>
    /// <param name="intendedVelocity">The target velocity to move towards.</param>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    protected void ApplyDampeners(Vector3 intendedVelocity, float deltaTime)
    {
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity,
            intendedVelocity, StrafeThrust * deltaTime);
    }

    protected void MoveTowards(Vector3 direction, float deltaTime)
    {
        if (UpdateRotation) Face(direction, deltaTime);
        else Face(RotationTarget.GetTargetRotation(transform), deltaTime);

        Vector3 input = transform.InverseTransformDirection(direction.normalized);

        if (Vector3.Angle(transform.forward, direction) > angleToForward)
            input.z = 0f; // don't apply forward thrust unless roughly facing target

        ApplyThrust(input, deltaTime);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (Destination != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, Destination.Value);
        }
        if (checker != null)
        {
            Gizmos.color = Color.white;
            checker.Draw(NavPivot != null ? NavPivot : transform, buffer);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, LinearVelocity);
    }
}
