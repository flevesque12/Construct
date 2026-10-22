using Construct.Input;
using UnityEngine;

namespace Construct.Cameras
{
    // Turns Look input into a yaw/pitch pivot that the over-the-shoulder aim
    // camera follows (CinemachineThirdPersonFollow copies the pivot's rotation).
    [DefaultExecutionOrder(100)] // after the player has moved, before the brain places the camera
    public sealed class AimCameraInput : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [Tooltip("The transform the aim camera follows. This script owns its position and rotation.")]
        [SerializeField] private Transform _pivot;
        [Tooltip("Where the pivot sits, usually the player's CameraTarget.")]
        [SerializeField] private Transform _followPoint;

        [Header("Mouse")]
        [Tooltip("Degrees per pixel. Lower than the orbit, aiming needs precision.")]
        [SerializeField] private float _mouseSensitivity = 0.1f;

        [Header("Gamepad")]
        [Tooltip("Degrees per second at full stick.")]
        [SerializeField] private float _gamepadSpeedX = 160f;
        [SerializeField] private float _gamepadSpeedY = 110f;

        [Header("Pitch")]
        [SerializeField] private float _minPitch = -40f;
        [SerializeField] private float _maxPitch = 50f;
        [SerializeField] private bool _invertY;

        [Header("Aim assist")]
        [Tooltip("The real camera: the assist aims the screen center, which is offset from the pivot.")]
        [SerializeField] private Transform _viewTransform;
        [Tooltip("How fast the aim is pulled toward the target (per second). Weak on mouse...")]
        [SerializeField] private float _mouseAssistStrength = 2f;
        [Tooltip("...stronger on gamepad, where precise aiming is harder.")]
        [SerializeField] private float _gamepadAssistStrength = 8f;

        private float _yaw;
        private float _pitch;
        private Transform _assistTarget;

        public void SetAngles(float yaw, float pitch)
        {
            _yaw = yaw;
            _pitch = Mathf.Clamp(pitch, _minPitch, _maxPitch);
            ApplyPivot();
        }

        public void SetAssistTarget(Transform target)
        {
            _assistTarget = target;
        }

        private void Update()
        {
            Vector2 look = _input.Look;
            Vector2 delta;

            if (_input.IsLookFromPointer)
            {
                // Mouse deltas are already frame-based, so no deltaTime here
                delta = Cursor.lockState == CursorLockMode.Locked ? look * _mouseSensitivity : Vector2.zero;
            }
            else
            {
                // Unscaled: the aim slow-mo must not slow the camera down
                delta = new Vector2(look.x * _gamepadSpeedX, look.y * _gamepadSpeedY) * Time.unscaledDeltaTime;
            }

            // Pushing up looks up, which is a negative pitch in Unity's Euler angles
            if (!_invertY)
                delta.y = -delta.y;

            _yaw += delta.x;
            _pitch += delta.y;
            ApplyAssist();
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            ApplyPivot();
        }

        // Magnetism: ease the angles toward the target a little every frame. The player can
        // still pull away, the assist only wins when the input is small.
        private void ApplyAssist()
        {
            if (_assistTarget == null)
                return;

            Vector3 toTarget = _assistTarget.position - _viewTransform.position;
            if (toTarget.sqrMagnitude < 0.01f)
                return;

            Vector3 desired = Quaternion.LookRotation(toTarget).eulerAngles;
            float strength = _input.IsLookFromPointer ? _mouseAssistStrength : _gamepadAssistStrength;
            // Frame-rate independent smoothing: same pull per second at 30 or 144 fps
            float blend = 1f - Mathf.Exp(-strength * Time.unscaledDeltaTime);

            _yaw = Mathf.LerpAngle(_yaw, desired.y, blend);
            _pitch = Mathf.LerpAngle(_pitch, Mathf.DeltaAngle(0f, desired.x), blend);
        }

        private void ApplyPivot()
        {
            _pivot.SetPositionAndRotation(_followPoint.position, Quaternion.Euler(_pitch, _yaw, 0f));
        }
    }
}
