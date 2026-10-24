using Construct.Combat;
using Construct.Input;
using UnityEngine;

namespace Construct.Player
{
    // Plays the light combo. Each hit runs on a timer (normalized 0..1) against its AttackData:
    // the hitbox is live during the hit window, a buffered press inside the combo window chains
    // the next hit, and from the cancel point on, moving gets the player out early.
    public sealed class AttackState : PlayerState
    {
        private AttackData _attack;
        private float _elapsed;
        private bool _swingDone;

        public AttackState(PlayerController ctx) : base(ctx) { }

        public override void Enter()
        {
            Ctx.Combo.Reset();
            StartNextAttack();
        }

        public override void Exit()
        {
            Ctx.Hitbox.Deactivate();
            Ctx.Combo.Reset();
            Ctx.Animator.PlayLocomotion();
        }

        public override void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            float normalizedTime = _elapsed / _attack.Duration;

            // KERNEL plants its feet: no steering, but gravity still applies
            Ctx.Motor.Move(Vector3.zero, 0f, deltaTime);
            UpdateHitbox(normalizedTime);

            if (Ctx.Combo.HasNext && _attack.IsInComboWindow(normalizedTime)
                && Ctx.InputBuffer.TryConsume(InputCommand.LightAttack))
            {
                StartNextAttack();
                return;
            }

            bool finished = normalizedTime >= 1f;
            bool cancelledByMove = normalizedTime >= _attack.CancelStart && Ctx.Input.Move.sqrMagnitude > 0.01f;
            if (finished || cancelledByMove)
            {
                Ctx.States.ChangeState(Ctx.Locomotion);
                return;
            }
        }

        private void StartNextAttack()
        {
            Ctx.Hitbox.Deactivate();
            _attack = Ctx.Combo.Next();
            _elapsed = 0f;
            _swingDone = false;
            FaceAttackDirection();
            Ctx.Animator.PlayAttack(_attack);
        }

        // Written with "passed the start / passed the end" rather than "inside the window",
        // so a long frame that jumps over the whole window still sweeps once
        private void UpdateHitbox(float normalizedTime)
        {
            if (!_swingDone && !Ctx.Hitbox.IsActive && normalizedTime >= _attack.HitStart)
                Ctx.Hitbox.Activate(_attack);

            if (!Ctx.Hitbox.IsActive)
                return;

            Ctx.Hitbox.Sweep();
            if (normalizedTime > _attack.HitEnd)
            {
                Ctx.Hitbox.Deactivate();
                _swingDone = true;
            }
        }

        // Each hit can be aimed: at the locked target, else where the stick points, else straight ahead
        private void FaceAttackDirection()
        {
            Vector3 direction = Ctx.LockOn.HasTarget
                ? Ctx.LockOn.CurrentTarget.AimPoint.position - Ctx.transform.position
                : Ctx.GetCameraRelativeMove();

            Ctx.Motor.SnapFacing(direction);
        }
    }
}
