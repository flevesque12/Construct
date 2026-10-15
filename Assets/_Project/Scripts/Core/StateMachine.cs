using UnityEngine;
using System;
using Construct.Core;

namespace Construct.Core
{
    public sealed class StateMachine
    {
        public IState Current {  get; private set; }

        public event Action<IState, IState> StateChanged;

        public void ChangeState(IState next)
        {
            if (next == null)
                throw new ArgumentNullException(nameof(next));

            if (next == Current)
                return;

            IState previous = Current;

            Current?.Exit();
            Current = next;
            Current.Enter();

            StateChanged?.Invoke(previous, next);
        }

        public void Tick(float deltaTime)
        {
            Current?.Tick(deltaTime);
        }
    }
}
