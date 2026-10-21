using UnityEngine;
using Construct.Core;
using Construct.Input;
using Construct.Weapon;

namespace Construct.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    [RequireComponent(typeof(LockOnSystem))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private LookFlickDetector _targetSwitch = new LookFlickDetector();

        [Header("Lock-on")]
        [Tooltip("Speed multiplier while strafing around a locked target.")]
        [Range(0f, 1f)]
        [SerializeField] private float _lockOnSpeedMultiplier = 0.85f;

        public InputReader Input => _input;
        public float LockOnSpeedMultiplier => _lockOnSpeedMultiplier;
        public PlayerMotor Motor { get; private set; }
        public LockOnSystem LockOn { get; private set; }
        public StateMachine States { get; private set; }

        // States, created once and reused
        public LocomotionState Locomotion { get; private set; }

        private void Awake()
        {
            Motor = GetComponent<PlayerMotor>();
            LockOn = GetComponent<LockOnSystem>();
            States = new StateMachine();
            Locomotion = new LocomotionState(this);
        }

        private void OnEnable()
        {
            _input.EnableGameplay();
            // Lock-on works in every state, so the controller owns it, not a state
            _input.LockOnPressed += LockOn.Toggle;
        }

        private void OnDisable()
        {
            _input.LockOnPressed -= LockOn.Toggle;
            _input.DisableGameplay();
        }

        private void Start()
        {
            States.ChangeState(Locomotion);
        }

        private void Update()
        {
            UpdateTargetSwitch(Time.deltaTime);
            States.Tick(Time.deltaTime);
        }

        private void UpdateTargetSwitch(float deltaTime)
        {
            if (!LockOn.HasTarget)
            {
                _targetSwitch.Reset();
                return;
            }

            if (_targetSwitch.TryGetFlick(_input.Look, _input.IsLookFromPointer, deltaTime, out Vector2 direction))
                LockOn.TrySwitch(direction);
        }

        public Vector3 GetCameraRelativeMove()
        {
            Vector2 input = _input.Move;

            Vector3 forward = _cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = _cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            return forward * input.y + right * input.x;
        }
    }
}