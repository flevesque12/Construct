using System;
using UnityEngine;

namespace Construct.Player
{
    // Turns continuous Look input into discrete "flicks", used to switch lock-on targets.
    // A stick flick fires once per push; a mouse flick fires on a quick swipe.
    [Serializable]
    public sealed class LookFlickDetector
    {
        [Header("Gamepad")]
        [Tooltip("Stick magnitude that counts as a flick.")]
        [Range(0f, 1f)]
        [SerializeField] private float _stickThreshold = 0.7f;
        [Tooltip("The stick must come back under this before it can flick again.")]
        [Range(0f, 1f)]
        [SerializeField] private float _stickReset = 0.3f;

        [Header("Mouse")]
        [Tooltip("Pixels the mouse must travel within the window to count as a flick.")]
        [SerializeField] private float _mouseThreshold = 60f;
        [SerializeField] private float _mouseWindow = 0.15f;

        [Tooltip("Minimum time between two flicks, for both devices.")]
        [SerializeField] private float _cooldown = 0.2f;

        private bool _stickArmed = true;
        private Vector2 _mouseAccumulated;
        private float _mouseWindowTimer;
        private float _cooldownTimer;

        public bool TryGetFlick(Vector2 look, bool isPointer, float deltaTime, out Vector2 direction)
        {
            direction = Vector2.zero;
            _cooldownTimer -= deltaTime;

            Vector2 flick = isPointer ? ReadMouse(look, deltaTime) : ReadStick(look);
            if (flick == Vector2.zero || _cooldownTimer > 0f)
                return false;

            _cooldownTimer = _cooldown;
            direction = flick.normalized;
            return true;
        }

        public void Reset()
        {
            _stickArmed = true;
            _mouseAccumulated = Vector2.zero;
            _mouseWindowTimer = 0f;
        }

        private Vector2 ReadStick(Vector2 look)
        {
            float magnitude = look.magnitude;

            if (magnitude < _stickReset)
                _stickArmed = true;

            if (!_stickArmed || magnitude < _stickThreshold)
                return Vector2.zero;

            _stickArmed = false;
            return look;
        }

        private Vector2 ReadMouse(Vector2 delta, float deltaTime)
        {
            // Only movement within the last window counts, slow drifts never add up to a flick
            _mouseWindowTimer += deltaTime;
            if (_mouseWindowTimer > _mouseWindow)
            {
                _mouseAccumulated = Vector2.zero;
                _mouseWindowTimer = 0f;
            }

            _mouseAccumulated += delta;
            if (_mouseAccumulated.magnitude < _mouseThreshold)
                return Vector2.zero;

            Vector2 flick = _mouseAccumulated;
            _mouseAccumulated = Vector2.zero;
            _mouseWindowTimer = 0f;
            return flick;
        }
    }
}
