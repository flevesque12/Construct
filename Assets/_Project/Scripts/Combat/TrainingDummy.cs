using UnityEngine;
using Construct.Core;


namespace Construct.Combat
{
    public sealed class TrainingDummy : MonoBehaviour, ITargetable
    {
        [SerializeField] private Transform aimPoint;
        
        public Transform AimPoint => aimPoint != null ? aimPoint : transform;
        public bool IsAlive => isActiveAndEnabled;
    }
}