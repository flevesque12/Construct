using System;
using UnityEngine;

namespace Construct.Combat
{
    // Pressure gauge on enemies (FF7 Remake's stagger): it fills with each hit, and when full
    // the enemy is Breached, which opens the hack. What a Breach does comes in phase 09.
    public sealed class Integrity : MonoBehaviour
    {
        [Min(1f)]
        [SerializeField] private float _max = 30f;

        public float Max => _max;
        public float Current { get; private set; }
        public float Normalized => Current / _max;
        public bool IsBreached { get; private set; }

        public event Action Breached;

        public void Add(float amount)
        {
            if (IsBreached || amount <= 0f)
                return;

            Current = Mathf.Min(_max, Current + amount);
            if (Current >= _max)
            {
                IsBreached = true;
                Breached?.Invoke();
            }
        }

        // Back to empty once the Breach is over (the hack failed, or the stun ended)
        public void ResetGauge()
        {
            Current = 0f;
            IsBreached = false;
        }
    }
}
