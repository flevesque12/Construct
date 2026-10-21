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

        public Transform AimPoint => _aimPoint != null ? _aimPoint : transform;
        public bool IsAlive => isActiveAndEnabled;
    }
}
