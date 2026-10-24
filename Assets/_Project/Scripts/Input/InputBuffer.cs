using UnityEngine;

namespace Construct.Input
{
    // Command pattern, lite: a press is stored as a command and executed later, when the
    // game is ready for it (e.g. the next combo hit once the window opens). Only the latest
    // press is kept, and it expires after a short window so mashing early does nothing.
    public sealed class InputBuffer
    {
        private readonly float _window;
        private InputCommand _command;
        private float _pressTime;
        private bool _hasCommand;

        public InputBuffer(float window)
        {
            _window = window;
        }

        public void Push(InputCommand command)
        {
            _command = command;
            // Real time: hitstop and slow-mo must not stretch the buffer
            _pressTime = Time.unscaledTime;
            _hasCommand = true;
        }

        public bool Has(InputCommand command)
        {
            return _hasCommand && _command == command && Time.unscaledTime - _pressTime <= _window;
        }

        public bool TryConsume(InputCommand command)
        {
            if (!Has(command))
                return false;

            _hasCommand = false;
            return true;
        }

        public void Clear()
        {
            _hasCommand = false;
        }
    }
}
