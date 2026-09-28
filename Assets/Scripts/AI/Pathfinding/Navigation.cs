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
    [field: SerializeField] public float StopDistance { get; set; } = 3f;
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
            //Debugging.LogWarning($"Created new nav pivot for {transform}.");
        }
        radAngleToForward = Mathf.Rad2Deg * angleToForward / 2;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GlobalUpdater.TryGetInstance(true).RegisterUpdate(PerformUpdate);
        //Destination = Vector3.zero;
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
            if (Dampeners) ApplyThrust(Vector3.zero, deltaTime);
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
        if (!checker.CheckForward(this, direction.normalized, buffer))
        {
            MoveTowards(direction, deltaTime);
        }
        else
        {
            //Debug.Log("Obstacle detected!");
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

    protected void MoveTowards(Vector3 direction, float deltaTime)
    {
        if (UpdateRotation) Face(direction, deltaTime);
        else Face(RotationTarget.GetTargetRotation(transform), deltaTime);

        direction -= LinearVelocity;
        var z = Vector3.Project(direction, Forward);
        float zInput;
        if (Vector3.Dot(z.normalized, Forward) > 0)
        {
            //apply forward thrust
            zInput = Mathf.Min(1, z.magnitude / ForwardThrust);
        }
        else
        {
            zInput = -Mathf.Min(1, z.magnitude / StrafeThrust);
        }

        var x = Vector3.Project(direction, Right);
        //Debug.DrawRay(Position, x, Color.magenta);
        var xInput = Vector3.Dot(x.normalized, Right) * Mathf.Min(1, x.magnitude / StrafeThrust);
        //Debug.Log(xInput);

        var y = Vector3.Project(direction, Up);
        var yInput = Vector3.Dot(y.normalized, Up) * Mathf.Min(1, y.magnitude / StrafeThrust);

        //Debug.DrawRay(Position, 10 * (zInput * Forward + xInput * Right + yInput * Up));

        ApplyThrust(new(xInput, yInput, zInput), deltaTime);
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
