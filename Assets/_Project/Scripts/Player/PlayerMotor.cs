using UnityEngine;

namespace Construct.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _acceleration = 40f;
        [SerializeField] private float _rotationSpeed = 720f;

        [Header("Jump & Gravity")]
        [SerializeField] private float _jumpHeight = 1.4f;
        [SerializeField] private float _gravity = -25f;
        [SerializeField] private float _groundedStickForce = -2f;

        private CharacterController _controller;
        private Vector3 _horizontalVelocity;
        private float _verticalVelocity;

        public bool IsGrounded => _controller.isGrounded;
        public Vector3 Velocity => _horizontalVelocity + Vector3.up * _verticalVelocity;
        // Top horizontal speed allowed by the last Move call (base speed × multiplier)
        public float MaxSpeed { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public void Move(Vector3 direction, float speedMultiplier, float deltaTime)
        {
            direction.y = 0f;
            direction = Vector3.ClampMagnitude(direction, 1f);

            MaxSpeed = _moveSpeed * speedMultiplier;
            Vector3 targetVelocity = direction * MaxSpeed;
            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity, targetVelocity, _acceleration * deltaTime);

            ApplyGravity(deltaTime);

            Vector3 motion = _horizontalVelocity + Vector3.up * _verticalVelocity;
            _controller.Move(motion * deltaTime);
        }

        public void FaceDirection(Vector3 direction, float deltaTime)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * deltaTime);
        }

        // Instant turn, for actions that must commit to a direction (attacks)
        public void SnapFacing(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            transform.rotation = Quaternion.LookRotation(direction);
        }

        public void Jump()
        {
            if (!IsGrounded)
                return;

            _verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
        }

        private void ApplyGravity(float deltaTime)
        {
            if (IsGrounded && _verticalVelocity < 0f)
                _verticalVelocity = _groundedStickForce;
            else
                _verticalVelocity += _gravity * deltaTime;
        }
    }
}
