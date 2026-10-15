using UnityEngine;
using System;
using UnityEngine.InputSystem;

namespace Construct.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Construct/InputReader")]
    public sealed class InputReader : ScriptableObject, GameControls.IGameplayActions
    {
        // Continuous values, read every frame by the states
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool IsThrowHeld { get; private set; }

        // One-shot button events
        public event Action JumpPressed;
        public event Action LightAttackPressed;
        public event Action HeavyAttackPressed;
        public event Action DodgePressed;
        public event Action LockOnPressed;
        public event Action ThrowPressed;
        public event Action ThrowReleased;
        public event Action FirePressed;

        private GameControls _controls;

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new GameControls();
                _controls.Gameplay.SetCallbacks(this);
            }
        }

        private void OnDisable()
        {
            DisableGameplay();
        }

        public void EnableGameplay() => _controls.Gameplay.Enable();
        public void DisableGameplay() => _controls?.Gameplay.Disable();

        public void OnMove(InputAction.CallbackContext context) => Move = context.ReadValue<Vector2>();
        public void OnLook(InputAction.CallbackContext context) => Look = context.ReadValue<Vector2>();

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) JumpPressed?.Invoke();
        }

        public void OnLightAttack(InputAction.CallbackContext context)
        {
            if (context.performed) LightAttackPressed?.Invoke();
        }

        public void OnHeavyAttack(InputAction.CallbackContext context)
        {
            if (context.performed) HeavyAttackPressed?.Invoke();
        }

        public void OnDodge(InputAction.CallbackContext context)
        {
            if (context.performed) DodgePressed?.Invoke();
        }

        public void OnLockOn(InputAction.CallbackContext context)
        {
            if (context.performed) LockOnPressed?.Invoke();
        }

        public void OnThrow(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                IsThrowHeld = true;
                ThrowPressed?.Invoke();
            }
            else if (context.canceled)
            {
                IsThrowHeld = false;
                ThrowReleased?.Invoke();
            }
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed) FirePressed?.Invoke();
        }
    }
}
