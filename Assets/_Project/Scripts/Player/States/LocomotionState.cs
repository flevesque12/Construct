using Construct.Input;
using UnityEngine;

namespace Construct.Player
{
    public sealed class LocomotionState : PlayerState
    {
        private float _throwHeldTime;

        public LocomotionState(PlayerController ctx) : base(ctx){ }

        public override void Enter()
        {
            _throwHeldTime = 0f;
            Ctx.Input.JumpPressed += OnJumpPressed;
        }

        public override void Exit()
        {
            Ctx.Input.JumpPressed -= OnJumpPressed;
        }

        public override void Tick(float deltaTime)
        {
            if (Ctx.Combo.HasNext && Ctx.InputBuffer.TryConsume(InputCommand.LightAttack))
            {
                Ctx.States.ChangeState(Ctx.Attack);
                return;
            }

            // Throw is a tap (quick throw, phase 10) or a hold (aim): only the hold time tells them apart
            if (Ctx.Input.IsThrowHeld)
            {
                _throwHeldTime += deltaTime;
                if (_throwHeldTime >= Ctx.AimHoldThreshold)
                {
                    Ctx.States.ChangeState(Ctx.Aim);
                    return;
                }
            }
            else
            {
                _throwHeldTime = 0f;
            }

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