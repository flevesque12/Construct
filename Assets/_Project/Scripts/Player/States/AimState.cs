using UnityEngine;

namespace Construct.Player
{
    // Throw held: over-the-shoulder camera, slower movement, KERNEL faces where the camera aims.
    public sealed class AimState : PlayerState
    {
        public AimState(PlayerController ctx) : base(ctx) { }

        public override void Enter()
        {
            Ctx.CameraRig?.BeginAim();
            Ctx.AimSystem.BeginAim();
        }

        public override void Exit()
        {
            Ctx.AimSystem.EndAim();
            Ctx.CameraRig?.EndAim();
        }

        public override void Tick(float deltaTime)
        {
            // Releasing the trigger cancels the aim; throwing with Fire comes with the sword (phase 10)
            if (!Ctx.Input.IsThrowHeld)
            {
                Ctx.States.ChangeState(Ctx.Locomotion);
                return;
            }

            // The AimSystem picks the target, the camera rig does the pulling
            Ctx.CameraRig?.SetAimAssistTarget(Ctx.AimSystem.AssistTarget);

            Vector3 moveDirection = Ctx.GetCameraRelativeMove();
            Ctx.Motor.Move(moveDirection, Ctx.AimSpeedMultiplier, deltaTime);
            Ctx.Motor.FaceDirection(Ctx.GetCameraForward(), deltaTime);
        }
    }
}
