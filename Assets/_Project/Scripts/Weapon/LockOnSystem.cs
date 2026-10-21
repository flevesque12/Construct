using System;
using Construct.Core;
using UnityEngine;

namespace Construct.Weapon
{
    // Picks the best lock-on target and keeps it valid. It never reads input:
    // the player decides when to lock, this system only decides who.
    public sealed class LockOnSystem : MonoBehaviour
    {
        [Tooltip("Usually the main camera: targets are scored against where the player is looking.")]
        [SerializeField] private Transform _viewTransform;
        [SerializeField] private LayerMask _targetMask;
        [Tooltip("What blocks line of sight to a target.")]
        [SerializeField] private LayerMask _obstacleMask;

        [Header("Range")]
        [SerializeField] private float _acquireRange = 15f;
        [Tooltip("Larger than the acquire range so the lock doesn't flicker at the edge.")]
        [SerializeField] private float _releaseRange = 20f;

        [Header("Scoring")]
        [Tooltip("Targets further than this angle from the view direction are ignored.")]
        [Range(0f, 180f)]
        [SerializeField] private float _maxAngle = 70f;
        [Tooltip("0 = closest target wins, 1 = most centered target wins.")]
        [Range(0f, 1f)]
        [SerializeField] private float _angleWeight = 0.7f;

        // Reused every search, so locking on allocates nothing
        private readonly Collider[] _hits = new Collider[16];

        public ITargetable CurrentTarget { get; private set; }
        public bool HasTarget => CurrentTarget != null;

        public event Action<ITargetable> TargetChanged;

        public void Toggle()
        {
            if (HasTarget)
                Release();
            else
                TryLock();
        }

        public bool TryLock()
        {
            ITargetable best = FindBestTarget();
            if (best == null)
                return false;

            SetTarget(best);
            return true;
        }

        public void Release()
        {
            SetTarget(null);
        }

        private void Update()
        {
            if (HasTarget && !IsStillValid(CurrentTarget))
                Release();
        }

        private ITargetable FindBestTarget()
        {
            Vector3 origin = transform.position;
            Vector3 viewForward = Flatten(_viewTransform.forward);

            int count = Physics.OverlapSphereNonAlloc(
                origin, _acquireRange, _hits, _targetMask, QueryTriggerInteraction.Ignore);

            ITargetable best = null;
            float bestScore = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                ITargetable candidate = _hits[i].GetComponentInParent<ITargetable>();
                if (candidate == null || !candidate.IsAlive)
                    continue;

                Vector3 toTarget = candidate.AimPoint.position - origin;
                float angle = Vector3.Angle(viewForward, Flatten(toTarget));
                if (angle > _maxAngle || !HasLineOfSight(candidate))
                    continue;

                // Both terms normalized to 0..1, lower is better
                float distanceScore = toTarget.magnitude / _acquireRange;
                float angleScore = angle / _maxAngle;
                float score = Mathf.Lerp(distanceScore, angleScore, _angleWeight);

                if (score < bestScore)
                {
                    bestScore = score;
                    best = candidate;
                }
            }

            return best;
        }

        private bool IsStillValid(ITargetable target)
        {
            // A destroyed MonoBehaviour is not null for C#, only for Unity's == operator
            if (target is UnityEngine.Object unityObject && unityObject == null)
                return false;

            if (!target.IsAlive)
                return false;

            float sqrDistance = (target.AimPoint.position - transform.position).sqrMagnitude;
            return sqrDistance <= _releaseRange * _releaseRange;
        }

        private bool HasLineOfSight(ITargetable target)
        {
            return !Physics.Linecast(
                _viewTransform.position, target.AimPoint.position, _obstacleMask, QueryTriggerInteraction.Ignore);
        }

        private void SetTarget(ITargetable target)
        {
            if (target == CurrentTarget)
                return;

            CurrentTarget = target;
            TargetChanged?.Invoke(CurrentTarget);
        }

        private static Vector3 Flatten(Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _acquireRange);
        }

        private void OnDrawGizmos()
        {
            if (!HasTarget || !IsStillValid(CurrentTarget))
                return;

            // Temporary target marker until the HUD exists
            Gizmos.color = Color.magenta;
            Vector3 aimPoint = CurrentTarget.AimPoint.position;
            Gizmos.DrawWireSphere(aimPoint, 0.35f);
            Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, aimPoint);
        }
    }
}
