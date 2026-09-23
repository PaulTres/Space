using UnityEngine;
using Utilities;

[RequireComponent(typeof(Rigidbody))]
public abstract class Ship : Unit
{
    #region Fields
    public float ForwardThrust => Parameters.ForwardThrust;
    public float RotationSpeed => Parameters.RotationSpeed;
    public float StrafeThrust => Parameters.StrafeThrust;
    protected Rigidbody rb;
    protected Vector3 angularVelocity;
    public float VelocitySoftCap => Parameters.VelocitySoftCap;
    public override Vector3 AngularVelocity => angularVelocity;
    public override Vector3 LinearVelocity => rb != null ? rb.linearVelocity : Vector3.zero;
    [field: SerializeField] public bool Dampeners { get; protected set; } = true;
    protected ThrusterGroup thrusters;
    protected AudioSource engineAudioSource;
    #endregion

    #region Setup
    protected override void Awake()
    {
        base.Awake();
        ComponentRegister<ImprovedMonoBehaviour>.Register(transform, this);
        thrusters = GetComponentInChildren<ThrusterGroup>();
        if (thrusters != null) engineAudioSource = thrusters.transform.GetComponent<AudioSource>();
        rb = Transform.GetOrAddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.linearDamping = 0;
        var speedModifier = GlobalConfig.GetUnitSpeedModifier(Team);
        Parameters.StrafeThrust *= speedModifier;
        Parameters.ForwardThrust *= speedModifier;
        Parameters.RotationSpeed *= speedModifier;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
    }
    #endregion

    protected void ApplyThrust(Vector3 movementVector, float deltaTime)
    {
        Vector3 intendedVelocity;
        if (!Dampeners)
        {
            intendedVelocity = movementVector.z * ForwardThrust * Forward +
                movementVector.x * StrafeThrust * Right +
                movementVector.y * StrafeThrust * Up;
        }
        else
        {
            intendedVelocity = Vector3.zero;
            if (movementVector.x == 0)
            {
                //apply counterforce
                var error = Vector3.ClampMagnitude(Vector3.Project(LinearVelocity,
                    Right), StrafeThrust);
                intendedVelocity -= error;
            }

            if (movementVector.y == 0)
            {
                //apply counterforce
                var error = Vector3.ClampMagnitude(Vector3.Project(LinearVelocity,
                    Up), StrafeThrust);
                intendedVelocity -= error;
            }

            if (movementVector.z == 0)
            {
                var error = Vector3.Project(LinearVelocity, Forward);
                if (Vector3.Dot(error, Forward) < 0)
                {
                    error = Vector3.ClampMagnitude(error, StrafeThrust);
                }
                else error = Vector3.ClampMagnitude(error, ForwardThrust);
                intendedVelocity -= error;
            }

            intendedVelocity += movementVector.z * ForwardThrust * Forward +
                movementVector.x * StrafeThrust * Right +
                movementVector.y * StrafeThrust * Up;
            rb.AddForce(intendedVelocity, ForceMode.Acceleration);
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MaxVelocityMagnitude);
            if (thrusters != null)
            {
                if (movementVector.z > 0) thrusters.Increase(deltaTime);
                else thrusters.Decrease(deltaTime);
                if (engineAudioSource != null)
                {
                    if (movementVector.sqrMagnitude > 0)
                    {
                        if (!engineAudioSource.isPlaying) engineAudioSource.Play();
                    }
                    else if (engineAudioSource.isPlaying) engineAudioSource.Stop();
                }
            }

            var counterForceMultiplier = (LinearVelocity.magnitude - VelocitySoftCap) / (MaxVelocityMagnitude - VelocitySoftCap);
            if (counterForceMultiplier > 0)
            {
                var counterForce = counterForceMultiplier * GlobalConfig.VelocitySoftCapCoefficient * deltaTime * rb.linearVelocity;
                rb.linearVelocity -= counterForce * (MaxVelocityMagnitude - intendedVelocity.magnitude);
            }
        }
    }

    public override void Die(ObjectDestroyed @event)
    {
        Dampeners = false;
        base.Die(@event);
    }
}
