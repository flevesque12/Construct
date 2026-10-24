using Construct.Combat;
using UnityEngine;

namespace Construct.Player
{
    // The only class that touches the Animator. Locomotion is read from the motor every frame;
    // one-shot actions (attacks) are played on request by the states.
    [DefaultExecutionOrder(50)] // after PlayerController has moved the player this frame
    [RequireComponent(typeof(PlayerController))]
    public sealed class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int MoveXId = Animator.StringToHash("MoveX");
        private static readonly int MoveZId = Animator.StringToHash("MoveZ");
        private static readonly int IsStrafingId = Animator.StringToHash("IsStrafing");
        private static readonly int AttackSpeedId = Animator.StringToHash("AttackSpeed");
        private static readonly int LocomotionStateId = Animator.StringToHash("Locomotion");
        private static readonly int StrafeStateId = Animator.StringToHash("Strafe");

        [Tooltip("The Animator on the character model (child of the player).")]
        [SerializeField] private Animator _animator;
        [Tooltip("Smooths parameter changes so blends don't pop, in seconds.")]
        [SerializeField] private float _dampTime = 0.1f;
        [Tooltip("Cross-fade from an action back to locomotion, in seconds.")]
        [SerializeField] private float _returnToLocomotionDuration = 0.2f;

        private PlayerController _player;
        private PlayerMotor _motor;

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
            _motor = GetComponent<PlayerMotor>();

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
            _animator.SetBool(IsStrafingId, _player.IsStrafing);
        }

        public void PlayAttack(AttackData attack)
        {
            _animator.SetFloat(AttackSpeedId, attack.Speed);
            // Restart from frame 0 even if the same state is already playing
            _animator.CrossFadeInFixedTime(attack.StateHash, attack.TransitionDuration, 0, 0f);
        }

        public void PlayLocomotion()
        {
            int state = _player.IsStrafing ? StrafeStateId : LocomotionStateId;
            _animator.CrossFadeInFixedTime(state, _returnToLocomotionDuration);
        }
    }
}
