using System;
using Construct.Combat;
using UnityEngine;

namespace Construct.Player
{
    // Knows the attack chain and where the player is in it. It doesn't play anything:
    // AttackState asks it for the next attack and decides when to use it.
    [Serializable]
    public sealed class ComboController
    {
        [Tooltip("The light combo, in order. The last hit ends the chain.")]
        [SerializeField] private AttackData[] _lightCombo = new AttackData[0];

        private int _index = -1;

        public bool HasNext => _index + 1 < _lightCombo.Length;

        // Next attack of the chain, or null when the chain is over
        public AttackData Next()
        {
            if (!HasNext)
                return null;

            _index++;
            return _lightCombo[_index];
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
