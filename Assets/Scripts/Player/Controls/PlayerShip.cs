using Radar;
using UnityEngine;
using Utilities;
namespace Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerShip : Ship
    {
        #region Fields
        [Header("Player controls")]
        [SerializeField] protected PlayerTargetSystem targetSystem;
        [SerializeField] protected ControlsInputReader inputReader;
        [SerializeField] protected Transform shipBody;
        [SerializeField] protected Transform cam;
        [SerializeField] protected Transform freelookCamPivot;
        [SerializeField] protected Transform camPivot;
        [SerializeField] protected float zoomSpeed = 1;
        [SerializeField] protected float xSens = 10;
        [SerializeField] protected float ySens = 10;
        [SerializeField] protected float zSens = 10;
        protected Vector3 defaultCamPos;
        protected Vector3 movementVector;
        protected bool freeLook, dampeners;
        protected float rollInput, camXAngle, camYAngle, camZAngle;
        public bool FreeLook
        {
            get => freeLook;
            set
            {
                if (freeLook != value)
                {
                    freeLook = value;
                    //currentCameraAngles = CameraRotationPivot.localRotation.eulerAngles;
                    if (!freeLook)
                    {
                        camXAngle = camPivot.localRotation.eulerAngles.x;
                        camYAngle = camPivot.localRotation.eulerAngles.y;
                        camZAngle = camPivot.localRotation.eulerAngles.z;
                    }
                    else
                    {
                        camXAngle = freelookCamPivot.localRotation.eulerAngles.x;
                        camYAngle = freelookCamPivot.localRotation.eulerAngles.y;
                        camZAngle = freelookCamPivot.localRotation.eulerAngles.z;
                    }
                }
            }
        }
        public override Vector3 Forward => shipBody.forward;
        public override Vector3 Right => shipBody.right;
        public override Vector3 Up => shipBody.up;
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            defaultCamPos = cam.localPosition;
            if (targetSystem == null) targetSystem = GetComponentInChildren<PlayerTargetSystem>();
            inputReader.EnablePlayerActions();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            GameManager.Player = this;
            dampeners = true;
            FreeLook = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inputReader.MoveEvent += OnMove;
            inputReader.RollEvent += OnRoll;
            inputReader.ZoomEvent += OnZoom;
            inputReader.ResetEvent += OnResetCamera;
            inputReader.FreeLookToggleEvent += OnFreeLookToggle;
            inputReader.DampenerToggleEvent += OnDampenerToggle;
            inputReader.FindTargetEvent += OnFindTarget;
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformFixedUpdate);
            GlobalUpdater.TryGetInstance(true).RegisterUpdate(PerformUpdate);
            GlobalUpdater.TryGetInstance(true).RegisterLateUpdate(PerformLateUpdate);
            OnResetCamera();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            GameManager.Player = null;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inputReader.DisableCallbacks();

            var i = GlobalUpdater.TryGetInstance();
            if (i != null)
            {
                i.UnregisterFixedUpdate(PerformFixedUpdate);
                i.UnregisterUpdate(PerformUpdate);
                i.UnregisterLateUpdate(PerformLateUpdate);
            }
        }
        #endregion

        #region Functionality
        protected void OnFindTarget()
        {
            targetSystem.SelectTarget();
        }

        protected void OnZoom(float zoom)
        {
            cam.localPosition = new Vector3(cam.localPosition.x, cam.localPosition.y,
                cam.localPosition.z + zoom * zoomSpeed);
        }

        protected void OnResetCamera()
        {
            cam.localPosition = defaultCamPos;
            freelookCamPivot.localRotation = Quaternion.identity;
        }

        protected void OnMove(Vector3 movementVector)
        {
            this.movementVector = movementVector;
        }

        protected void OnRoll(float rollValue)
        {
            rollInput = rollValue;
        }

        protected void OnFreeLookToggle(float value) => FreeLook = value > .5f;

        protected void OnDampenerToggle() => dampeners = !dampeners;

        public void PerformFixedUpdate(float deltaTime)
        {
            Vector3 intendedVelocity;
            if (!dampeners)
            {
                intendedVelocity = movementVector.z * forwardThrust * shipBody.forward +
                    movementVector.x * strafeThrust * shipBody.right +
                    movementVector.y * strafeThrust * shipBody.up;
            }
            else
            {
                intendedVelocity = Vector3.zero;
                if (movementVector.x == 0)
                {
                    //apply counterforce
                    var error = Vector3.ClampMagnitude(Vector3.Project(LinearVelocity,
                        Right), strafeThrust);
                    intendedVelocity -= error;
                }

                if (movementVector.y == 0)
                {
                    //apply counterforce
                    var error = Vector3.ClampMagnitude(Vector3.Project(LinearVelocity,
                        Up), strafeThrust);
                    intendedVelocity -= error;
                }

                if (movementVector.z == 0)
                {
                    var error = Vector3.Project(LinearVelocity, Forward);
                    if (Vector3.Dot(error, Forward) < 0)
                    {
                        error = Vector3.ClampMagnitude(error, strafeThrust);
                    }
                    else error = Vector3.ClampMagnitude(error, forwardThrust);
                    intendedVelocity -= error;
                }

                intendedVelocity += movementVector.z * forwardThrust * shipBody.forward +
                    movementVector.x * strafeThrust * shipBody.right +
                    movementVector.y * strafeThrust * shipBody.up;
            }

            rb.AddForce(intendedVelocity, ForceMode.Acceleration);
            rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, MaxVelocityMagnitude);
        }

        public void PerformUpdate(float deltaTime)
        {
            if (!freeLook)
            {
                Quaternion previousRotation = shipBody.rotation;

                //rotate the ship body to match the rotation of the camera pivot
                shipBody.rotation = Quaternion.RotateTowards(shipBody.rotation, camPivot.rotation, deltaTime * RotationSpeed);

                Quaternion rotationDelta = (shipBody.rotation *
                    Quaternion.Inverse(previousRotation)).normalized;
                rotationDelta.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
                if (angleInDegrees > 180f) angleInDegrees -= 360f;
                angularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / deltaTime;
            }
        }

        public void PerformLateUpdate(float deltaTime)
        {
            camXAngle -= deltaTime * inputReader.LookDelta.y * xSens;
            camYAngle += deltaTime * inputReader.LookDelta.x * ySens;
            camZAngle += deltaTime * rollInput * zSens;

            if (freeLook)
            {
                freelookCamPivot.localRotation = Quaternion.Euler(camXAngle, camYAngle, camZAngle);
            }
            else camPivot.localRotation = Quaternion.Euler(camXAngle, camYAngle, camZAngle);
        }
        #endregion
    }
}
