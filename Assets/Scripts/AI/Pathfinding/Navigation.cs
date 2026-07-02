using UnityEngine;
using Utilities;
[RequireComponent(typeof(Rigidbody))]
public class Navigation : Ship
{
    [field: SerializeField] public bool DampenVelocityToZero { get; set; } = true;
    public void SetDestination(Vector3? destination) => Destination = destination;
    public Vector3? Destination { get; protected set; }
    public bool HasDestination => Destination != null;
    [Header("Parameters")]
    [SerializeField] protected Transform navPivot;
    [field: SerializeField] public float StopDistance { get; set; } = 0.5f;
    public float angleToForward = 10;
    protected float radAngleToForward;
    [SerializeField] protected ObstacleChecker checker;
    [field: SerializeField] public bool UpdateRotation { get; set; } = true;

    #region Setup
    protected override void Awake()
    {
        base.Awake();
        if (navPivot == null)
        {
            navPivot = new GameObject().transform;
            navPivot.SetPositionAndRotation(transform.position, transform.rotation);
            navPivot.parent = transform;
            Debug.LogWarning($"Created new nav pivot for {transform}.");
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
        if (GlobalUpdater.TryGetInstance() != null) GlobalUpdater.TryGetInstance().
                UnregisterUpdate(PerformUpdate);
    }
    #endregion

    protected void PerformUpdate(float deltaTime)
    {
        if (Destination == null)
        {
            if (DampenVelocityToZero) ApplyDampeners(Vector3.zero, deltaTime);
            return;
        }
        navPivot.LookAt(Destination.Value);
        Vector3 direction = Destination.Value - transform.position;
        if (direction.magnitude <= StopDistance || Vector3.Distance(transform.position,
            Destination.Value) <= StopDistance)
        {
            Destination = null;
            return;
        }
        if (!checker.CheckForward(transform, direction.normalized))
        {
            MoveTowards(direction, deltaTime);
        }
        else
        {
            var point = checker.RunCheck(navPivot);
            MoveTowards(point, deltaTime);
        }
    }

    public void Face(Vector3 direction, float deltaTime)
    {
        if (direction == Vector3.zero) return;

        Quaternion rot = Quaternion.LookRotation(direction, direction + transform.up);
        Quaternion previousRotation = transform.rotation;
        transform.rotation = Quaternion.RotateTowards(previousRotation, rot,
            rotationSpeed * deltaTime);

        Quaternion rotationDelta = (transform.rotation *
            Quaternion.Inverse(previousRotation)).normalized;
        rotationDelta.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
        if (angleInDegrees > 180f) angleInDegrees -= 360f;

        angularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / deltaTime;
    }

    protected void ApplyDampeners(Vector3 intendedVelocity, float deltaTime)
    {
        rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity,
            intendedVelocity, strafeThrust * deltaTime);
    }

    protected void MoveTowards(Vector3 direction, float deltaTime)
    {
        if (UpdateRotation) Face(direction, deltaTime);

        ApplyDampeners(direction, deltaTime);
        if (Vector3.Dot(transform.forward, direction) >= Mathf.Cos(Mathf.Deg2Rad *
            angleToForward / 2))
        {
            rb.AddForce(forwardThrust * Time.deltaTime * direction.normalized,
                ForceMode.Acceleration);
        }
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxVelocityMagnitude);
    }

    protected void OnDrawGizmos()
    {
        if (Destination != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, Destination.Value);
        }
        if (checker != null)
        {
            Gizmos.color = Color.white;
            checker.Draw(navPivot != null ? navPivot : transform);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, LinearVelocity);
    }
}
