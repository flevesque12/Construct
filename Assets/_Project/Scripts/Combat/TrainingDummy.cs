using Construct.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Construct.Combat
{
    public sealed class TrainingDummy : MonoBehaviour, ITargetable
    {
        // FormerlySerializedAs keeps the reference already set on the prefab
        [FormerlySerializedAs("aimPoint")]
        [SerializeField] private Transform _aimPoint;

        private Health _health;

        public Transform AimPoint => _aimPoint != null ? _aimPoint : transform;
        public bool IsAlive => isActiveAndEnabled;

        private void Awake()
        {
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (_health != null)
                _health.Died += OnDied;
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.Died -= OnDied;
        }

        // A training dummy never stays down
        private void OnDied()
        {
            _health.ResetToMax();
        }
    }
}
