using Construct.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace Construct.Cameras
{
    // The only class that turns gameplay camera requests into Cinemachine.
    // Three cameras: the free orbit, a lock camera that stays behind the player
    // facing the target, and an over-the-shoulder aim camera. Priorities decide
    // which one is live (aim 30 > lock 20 > orbit 10); the brain blends between them.
    [DefaultExecutionOrder(100)] // runs after the player has moved this frame
    public sealed class CinemachineCameraRig : MonoBehaviour, ICameraRig
    {
        [Tooltip("The real camera (Main Camera): aiming starts from where it looks.")]
        [SerializeField] private Transform _outputCamera;

        [Header("Orbit")]
        [SerializeField] private CinemachineCamera _orbitCamera;
        [SerializeField] private PlayerCameraInput _orbitInput;

        [Header("Lock-on")]
        [SerializeField] private CinemachineCamera _lockCamera;
        [Tooltip("The lock camera stays behind this point, usually the player's CameraTarget.")]
        [SerializeField] private Transform _pivot;
        [Tooltip("How fast the lock camera swings behind the player, in seconds.")]
        [SerializeField] private float _yawSmoothTime = 0.15f;

        [Header("Aim")]
        [SerializeField] private CinemachineCamera _aimCamera;
        [SerializeField] private AimCameraInput _aimInput;

        private CinemachineOrbitalFollow _orbitFollow;
        private CinemachineOrbitalFollow _lockFollow;
        private ITargetable _lockTarget;
        private bool _isAiming;
        private float _yawVelocity;

        private void Awake()
        {
            _orbitFollow = _orbitCamera.GetComponent<CinemachineOrbitalFollow>();
            _lockFollow = _lockCamera.GetComponent<CinemachineOrbitalFollow>();

            // When the orbit comes back, it starts from wherever the previous camera was,
            // so unlocking or ending the aim leaves the view where it was instead of snapping
            _orbitCamera.BlendHint |= CinemachineCore.BlendHints.InheritPosition;
            _lockCamera.enabled = false;
            _aimCamera.enabled = false;
            RefreshInputs();
        }

        public void BeginAim()
        {
            if (_isAiming)
                return;

            // Start the aim exactly where the player was looking
            Vector3 euler = _outputCamera.rotation.eulerAngles;
            _aimInput.SetAngles(euler.y, Mathf.DeltaAngle(0f, euler.x));

            _isAiming = true;
            _aimCamera.enabled = true;
            RefreshInputs();
        }

        public void EndAim()
        {
            if (!_isAiming)
                return;

            _isAiming = false;
            _aimInput.SetAssistTarget(null);
            _aimCamera.enabled = false;
            RefreshInputs();
        }

        public void SetAimAssistTarget(ITargetable target)
        {
            _aimInput.SetAssistTarget(target != null ? target.AimPoint : null);
        }

        // Only the live camera listens to Look; while locked, Look switches targets instead
        private void RefreshInputs()
        {
            _aimInput.enabled = _isAiming;
            _orbitInput.enabled = !_isAiming && _lockTarget == null;
        }

        public void SetLockTarget(ITargetable target)
        {
            if (target == null)
            {
                ClearLockTarget();
                return;
            }

            bool wasLocked = _lockTarget != null;
            _lockTarget = target;
            _lockCamera.LookAt = target.AimPoint;

            if (wasLocked)
                return;

            // Start from the orbit's angle, then swing behind the player in Update
            _lockFollow.HorizontalAxis.Value = _orbitFollow.HorizontalAxis.Value;
            _yawVelocity = 0f;

            _lockCamera.enabled = true;
            RefreshInputs();
        }

        public void ClearLockTarget()
        {
            if (_lockTarget == null)
                return;

            _lockTarget = null;
            _lockCamera.enabled = false;
            RefreshInputs();
        }

        private void Update()
        {
            if (_lockTarget == null)
                return;

            // The target can be destroyed before the lock-on system releases it
            if (_lockTarget is Object unityObject && unityObject == null)
                return;

            Vector3 toTarget = _lockTarget.AimPoint.position - _pivot.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.01f)
                return;

            // Yaw 0 puts the camera on -Z looking +Z, so the yaw of toTarget keeps it behind the player
            float desiredYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;

            ref InputAxis yaw = ref _lockFollow.HorizontalAxis;
            float smoothed = Mathf.SmoothDampAngle(yaw.Value, desiredYaw, ref _yawVelocity, _yawSmoothTime);
            yaw.Value = yaw.ClampValue(smoothed);
        }
    }
}
