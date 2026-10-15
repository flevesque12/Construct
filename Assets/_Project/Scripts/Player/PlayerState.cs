using Construct.Core;

namespace Construct.Player
{
    public abstract class PlayerState : IState
    {
        protected PlayerController Ctx { get; }

        protected PlayerState(PlayerController ctx)
        {
            Ctx = ctx;
        }

        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void Exit() { }
    }
}