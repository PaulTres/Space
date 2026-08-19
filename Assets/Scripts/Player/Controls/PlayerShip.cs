using EventBus;
using Level;
using Radar;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;
namespace Player
{
    public class PlayerShip : Ship, PlayerControls.IBasicControlsActions
    {
        [System.Serializable]
        public class CameraData
        {
            public Transform CameraPivot;
            public Transform FreelookCameraPivot;
            public Transform CameraTransform;
            public float ZoomSpeed = 1;
            [Range(-100, 0)]
            public float MinZoom = -10;
            [Range(0, 50)]
            public float MaxZoom = 5;
            public float XSens = 10;
            public float YSens = 10;
            public float ZSens = 10;
        }
        public static Camera PlayerCamera { get; protected set; } = null;

        #region Fields
        [Header("Player controls")]
        [SerializeField] protected Transform shipBody;
        [SerializeField] protected CameraData cameraData;
        [Header("UI")]
        [SerializeField] protected GameObject missionFailureScreenPrefab;
        protected Vector3 defaultCamPos;
        protected Vector3 movementVector;
        protected bool freeLook;
        protected float rollInput;
        protected Transform cam => cameraData.CameraTransform;
        public override Vector3 Forward => shipBody.forward;
        public override Vector3 Right => shipBody.right;
        public override Vector3 Up => shipBody.up;
        public override ImprovedMonoBehaviour Target
        {
            get
            {
                var t = PlayerTargetSystem.TryGetInstance();
                if (t != null) return t.CurrentTarget;
                return null;
            }
        }
        public bool CannotDie { get; set; } = false;
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            PlayerCamera = cameraData.CameraTransform.GetComponent<Camera>();
            defaultCamPos = cameraData.CameraTransform.localPosition;
            GameManager.InputActions.BasicControls.SetCallbacks(this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            CannotDie = false;
            PlayerCamera.fieldOfView = GlobalConfig.FOV;
            GameManager.Player = this;
            Dampeners = true;
            freeLook = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GlobalUpdater.TryGetInstance(true).RegisterFixedUpdate(PerformFixedUpdate);
            GlobalUpdater.TryGetInstance(true).RegisterUpdate(PerformUpdate);
            GlobalUpdater.TryGetInstance(true).RegisterLateUpdate(PerformLateUpdate);
            OnResetCamera();
            EventBus<PauseScreen.PauseEvent>.AddActions(OnPauseEvent);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            EventBus<PauseScreen.PauseEvent>.RemoveActions(OnPauseEvent);
            PlayerCamera = null;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            DisableCallbacks();
            var i = GlobalUpdater.TryGetInstance();
            if (i != null)
            {
                i.UnregisterFixedUpdate(PerformFixedUpdate);
                i.UnregisterUpdate(PerformUpdate);
                i.UnregisterLateUpdate(PerformLateUpdate);
            }
            if (IsDying)
            {
                //spawn mission failure screen
                Instantiate(missionFailureScreenPrefab, transform.position, transform.rotation);
            }
        }

        protected void DisableCallbacks()
        {
            GameManager.InputActions.BasicControls.RemoveCallbacks(this);
        }
        #endregion

        #region Functionality

        #region Controls
        public void OnPauseEvent(PauseScreen.PauseEvent pauseEvent)
        {
            if (pauseEvent.Paused) DisableCallbacks();
            else GameManager.InputActions.BasicControls.SetCallbacks(this);
        }

        public void OnFindTarget(InputAction.CallbackContext context)
        {
            if (context.started) PlayerTargetSystem.TryGetInstance().SelectTarget();
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            cam.localPosition = new Vector3(cam.localPosition.x, cam.localPosition.y,
               Mathf.Clamp(cam.localPosition.z + context.ReadValue<float>() * cameraData.ZoomSpeed, cameraData.MinZoom, cameraData.MaxZoom));
        }

        public void OnReset(InputAction.CallbackContext context)
        {
            if (context.started) OnResetCamera();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            movementVector = context.ReadValue<Vector3>();
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            rollInput = context.ReadValue<float>();
        }

        public void OnFreeLookToggle(InputAction.CallbackContext context)
        {
            if (context.started) freeLook = !freeLook;
        }

        public void OnToggleDampeners(InputAction.CallbackContext context)
        {
            if (context.started) Dampeners = !Dampeners;
        }

        public void OnMouseLook(InputAction.CallbackContext context) { }
        #endregion

        protected void OnResetCamera()
        {
            cam.localPosition = defaultCamPos;
            cameraData.FreelookCameraPivot.localRotation = Quaternion.identity;
        }

        public void PerformFixedUpdate(float deltaTime)
        {
            //Debug.Log(missileCount);
            Vector3 intendedVelocity;
            if (!Dampeners)
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
            if (thrusters != null)
            {
                if (movementVector.z > 0) thrusters.Increase(deltaTime);
                else thrusters.Decrease(deltaTime);
            }
        }

        public void PerformUpdate(float deltaTime)
        {
            if (!freeLook)
            {
                Quaternion previousRotation = shipBody.rotation;

                //rotate the ship body to match the rotation of the camera pivot
                shipBody.rotation = Quaternion.RotateTowards(shipBody.rotation, cameraData.CameraPivot.rotation, deltaTime * RotationSpeed);

                Quaternion rotationDelta = (shipBody.rotation *
                    Quaternion.Inverse(previousRotation)).normalized;
                rotationDelta.ToAngleAxis(out float angleInDegrees, out Vector3 axis);
                if (angleInDegrees > 180f) angleInDegrees -= 360f;
                angularVelocity = axis * (angleInDegrees * Mathf.Deg2Rad) / deltaTime;
            }
        }

        public void PerformLateUpdate(float deltaTime)
        {
            var lookDelta = GameManager.InputActions.BasicControls.MouseLook.ReadValue<Vector2>();
            if (freeLook)
            {
                cameraData.FreelookCameraPivot.Rotate(
                    -deltaTime * lookDelta.y * cameraData.XSens,
                    deltaTime * lookDelta.x * cameraData.YSens,
                    deltaTime * rollInput * cameraData.ZSens);
            }
            else
            {
                cameraData.CameraPivot.Rotate(
                    -deltaTime * lookDelta.y * cameraData.XSens,
                    deltaTime * lookDelta.x * cameraData.YSens,
                    deltaTime * rollInput * cameraData.ZSens);
            }
        }

        public override void Die(ObjectDestroyed @event)
        {
            if (CannotDie) return;
            GameManager.Player = null;
            MissionManager.TryGetInstance(true).StopAllTimers();
            UISystem.ShowText(new(Vector3.zero, 5, "Mission failed!", Color.red));
            DisableCallbacks();
            base.Die(@event);
        }
        #endregion
    }
}
