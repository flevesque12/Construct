using System;
using UnityEngine;

namespace Construct.Combat
{
    // Hit points, for the player, enemies and bosses alike. Knows nothing about who hits it.
    public sealed class Health : MonoBehaviour
    {
        [Min(1f)]
        [SerializeField] private float _max = 100f;

        public float Max => _max;
        public float Current { get; private set; }
        public float Normalized => Current / _max;
        public bool IsDead => Current <= 0f;

        public event Action<float> Damaged; // amount actually removed
        public event Action Died;

        private void Awake()
        {
            Current = _max;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            float before = Current;
            Current = Mathf.Max(0f, Current - amount);
            Damaged?.Invoke(before - Current);

            if (IsDead)
                Died?.Invoke();
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            Current = Mathf.Min(_max, Current + amount);
        }

        public void ResetToMax()
        {
            Current = _max;
        }
    }
}
