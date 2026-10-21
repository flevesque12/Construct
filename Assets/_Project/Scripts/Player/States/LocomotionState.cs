using UnityEngine;

namespace Construct.Player
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

            if (Ctx.LockOn.HasTarget)
            {
                // Strafe: keep moving where the stick points, but always face the target
                Vector3 toTarget = Ctx.LockOn.CurrentTarget.AimPoint.position - Ctx.transform.position;
                Ctx.Motor.Move(moveDirection, Ctx.LockOnSpeedMultiplier, deltaTime);
                Ctx.Motor.FaceDirection(toTarget, deltaTime);
                return;
            }

            Ctx.Motor.Move(moveDirection, 1f, deltaTime);
            Ctx.Motor.FaceDirection(moveDirection, deltaTime);
        }

        private void OnJumpPressed()
        {
            Ctx.Motor.Jump();
        }
    }
}