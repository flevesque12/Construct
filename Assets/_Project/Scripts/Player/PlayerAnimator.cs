using Construct.Weapon;
using UnityEngine;

namespace Construct.Player
{
    // Translates what the player is doing into Animator parameters.
    // States and the motor never touch the Animator: they move the player, this reads the result.
    [DefaultExecutionOrder(50)] // after PlayerController has moved the player this frame
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(LockOnSystem))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int MoveXId = Animator.StringToHash("MoveX");
        private static readonly int MoveZId = Animator.StringToHash("MoveZ");
        private static readonly int IsLockedOnId = Animator.StringToHash("IsLockedOn");

        [Tooltip("The Animator on the character model (child of the player).")]
        [SerializeField] private Animator _animator;
        [Tooltip("Smooths parameter changes so blends don't pop, in seconds.")]
        [SerializeField] private float _dampTime = 0.1f;

        private PlayerMotor _motor;
        private LockOnSystem _lockOn;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _lockOn = GetComponent<LockOnSystem>();

            // The CharacterController hovers by its skin width; sink the model by the same amount so the feet touch the ground
            float skinWidth = GetComponent<CharacterController>().skinWidth;
            _animator.transform.localPosition = Vector3.down * skinWidth;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            // Velocity in the player's own space, scaled so full speed = 1 whatever the multiplier
            Vector3 velocity = _motor.Velocity;
            velocity.y = 0f;
            Vector3 local = _motor.MaxSpeed > 0.01f ? transform.InverseTransformDirection(velocity) / _motor.MaxSpeed : Vector3.zero;

            _animator.SetFloat(SpeedId, Mathf.Clamp01(local.magnitude), _dampTime, deltaTime);
            _animator.SetFloat(MoveXId, local.x, _dampTime, deltaTime);
            _animator.SetFloat(MoveZId, local.z, _dampTime, deltaTime);
            _animator.SetBool(IsLockedOnId, _lockOn.HasTarget);
        }
    }
}
