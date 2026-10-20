using Construct.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace Construct.Cameras
{
    // Drives the orbit axes from the InputReader, with separate tuning for
    // mouse (per-frame deltas) and gamepad (rate-based stick).
    [RequireComponent(typeof(CinemachineOrbitalFollow))]
    public sealed class PlayerCameraInput : MonoBehaviour
    {
        [SerializeField] private InputReader _input;

        [Header("Mouse")]
        [Tooltip("Degrees of yaw per pixel of mouse movement.")]
        [SerializeField] private float _mouseSensitivityX = 0.15f;
        [Tooltip("Orbit height (0 = bottom ring, 1 = top ring) per pixel of mouse movement.")]
        [SerializeField] private float _mouseSensitivityY = 0.002f;

        [Header("Gamepad")]
        [Tooltip("Degrees of yaw per second at full stick.")]
        [SerializeField] private float _gamepadSpeedX = 200f;
        [Tooltip("Orbit height per second at full stick.")]
        [SerializeField] private float _gamepadSpeedY = 1.2f;

        [SerializeField] private bool _invertY;

        private CinemachineOrbitalFollow _orbit;

        private void Awake()
        {
            _orbit = GetComponent<CinemachineOrbitalFollow>();
        }

        private void Update()
        {
            Vector2 look = _input.Look;
            if (look == Vector2.zero)
                return;

            Vector2 delta;
            if (_input.IsLookFromPointer)
            {
                // Mouse deltas are already frame-based, so no deltaTime here
                if (Cursor.lockState != CursorLockMode.Locked)
                    return;

                delta = new Vector2(look.x * _mouseSensitivityX, look.y * _mouseSensitivityY);
            }
            else
            {
                delta = new Vector2(look.x * _gamepadSpeedX, look.y * _gamepadSpeedY) * Time.deltaTime;
            }

            // Pushing up tilts the view up, which moves the camera down its orbit
            if (!_invertY)
                delta.y = -delta.y;

            ref InputAxis horizontal = ref _orbit.HorizontalAxis;
            horizontal.Value = horizontal.ClampValue(horizontal.Value + delta.x);

            ref InputAxis vertical = ref _orbit.VerticalAxis;
            vertical.Value = vertical.ClampValue(vertical.Value + delta.y);
        }
    }
}
