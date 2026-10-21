using Construct.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace Construct.Cameras
{
    // The only class that turns gameplay camera requests into Cinemachine.
    // Two cameras: the free orbit, and a lock camera that stays behind the
    // player facing the target. The brain blends between them.
    [DefaultExecutionOrder(100)] // runs after the player has moved this frame
    public sealed class CinemachineCameraRig : MonoBehaviour, ICameraRig
    {
        [Header("Orbit")]
        [SerializeField] private CinemachineCamera _orbitCamera;
        [SerializeField] private PlayerCameraInput _orbitInput;

        [Header("Lock-on")]
        [SerializeField] private CinemachineCamera _lockCamera;
        [Tooltip("The lock camera stays behind this point, usually the player's CameraTarget.")]
        [SerializeField] private Transform _pivot;
        [Tooltip("How fast the lock camera swings behind the player, in seconds.")]
        [SerializeField] private float _yawSmoothTime = 0.15f;

        private CinemachineOrbitalFollow _orbitFollow;
        private CinemachineOrbitalFollow _lockFollow;
        private ITargetable _lockTarget;
        private float _yawVelocity;

        private void Awake()
        {
            _orbitFollow = _orbitCamera.GetComponent<CinemachineOrbitalFollow>();
            _lockFollow = _lockCamera.GetComponent<CinemachineOrbitalFollow>();

            // When the orbit comes back, it starts from wherever the lock camera was,
            // so unlocking leaves the view behind the player instead of snapping
            _orbitCamera.BlendHint |= CinemachineCore.BlendHints.InheritPosition;
            _lockCamera.enabled = false;
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

            // The mouse and stick now switch targets instead of orbiting
            _orbitInput.enabled = false;
            _lockCamera.enabled = true;
        }

        public void ClearLockTarget()
        {
            if (_lockTarget == null)
                return;

            _lockTarget = null;
            _lockCamera.enabled = false;
            _orbitInput.enabled = true;
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
