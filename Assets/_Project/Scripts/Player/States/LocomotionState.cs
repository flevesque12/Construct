using Construct.Player;
using UnityEngine;

namespace Contruct.Player
{
    public sealed class LocomotionState : PlayerState
    {
        public LocomotionState(PlayerController ctx) : base(ctx){ }

        public override void Enter()
        {
            Ctx.Input.JumpPressed += OnJumpPressed;
        }

        public override void Exit()
        {
            Ctx.Input.JumpPressed -= OnJumpPressed;
        }

        public override void Tick(float deltaTime)
        {
            Vector3 moveDirection = Ctx.GetCameraRelativeMove();

            Ctx.Motor.Move(moveDirection, 1f, deltaTime);
            Ctx.Motor.FaceDirection(moveDirection, deltaTime);
        }

        private void OnJumpPressed()
        {
            Ctx.Motor.Jump();
        }
    }
}