using UnityEngine;
using Construct.Core;
using Construct.Input;

namespace Construct.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private Transform _cameraTransform;

        public InputReader Input => _input;
        public PlayerMotor Motor { get; private set; }
        public StateMachine States { get; private set; }

        // States, created once and reused
        public LocomotionState Locomotion { get; private set; }

        private void Awake()
        {
            Motor = GetComponent<PlayerMotor>();
            States = new StateMachine();
            Locomotion = new LocomotionState(this);
        }

        private void OnEnable()
        {
            _input.EnableGameplay();
        }

        private void OnDisable()
        {
            _input.DisableGameplay();
        }

        private void Start()
        {
            States.ChangeState(Locomotion);
        }

        private void Update()
        {
            States.Tick(Time.deltaTime);
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