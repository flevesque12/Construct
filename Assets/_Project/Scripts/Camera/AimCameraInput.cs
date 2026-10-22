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

        private float _yaw;
        private float _pitch;

        public void SetAngles(float yaw, float pitch)
        {
            _yaw = yaw;
            _pitch = Mathf.Clamp(pitch, _minPitch, _maxPitch);
            ApplyPivot();
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
            _pitch = Mathf.Clamp(_pitch + delta.y, _minPitch, _maxPitch);
            ApplyPivot();
        }

        private void ApplyPivot()
        {
            _pivot.SetPositionAndRotation(_followPoint.position, Quaternion.Euler(_pitch, _yaw, 0f));
        }
    }
}
