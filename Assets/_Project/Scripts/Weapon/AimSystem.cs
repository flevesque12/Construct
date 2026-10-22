using System.Collections.Generic;
using Construct.Core;
using UnityEngine;

namespace Construct.Weapon
{
    // Answers "what would a throw do right now?" while the player aims, and tells the HUD
    // through the reticle channel. It never reads input: AimState calls BeginAim / EndAim.
    // It also slows time at the start of the aim and picks the aim-assist target.
    public sealed class AimSystem : MonoBehaviour
    {
        [Tooltip("Usually the main camera: the reticle is the center of the screen.")]
        [SerializeField] private Transform _viewTransform;
        [Tooltip("Where the sword leaves from. The range is measured from here, not from the camera.")]
        [SerializeField] private Transform _throwOrigin;
        [Tooltip("What the camera ray can land on: world geometry and targets.")]
        [SerializeField] private LayerMask _aimMask;
        [Tooltip("What blocks the throw between KERNEL and the target.")]
        [SerializeField] private LayerMask _obstacleMask;

        [SerializeField] private float _range = 8f;
        [Tooltip("How far the camera ray looks, beyond the throw range, to place the aim point.")]
        [SerializeField] private float _maxRayDistance = 60f;

        [SerializeField] private ReticleStateEventChannel _reticleChannel;

        [Header("Slow motion")]
        [SerializeField] private SlowMotion _slowMotion;
        [Range(0f, 1f)]
        [SerializeField] private float _slowMotionScale = 0.7f;
        [Tooltip("Real seconds of slow motion at the start of each aim.")]
        [SerializeField] private float _slowMotionDuration = 1.5f;

        [Header("Aim assist")]
        [Tooltip("Targets the assist can pull toward.")]
        [SerializeField] private LayerMask _assistMask;
        [Tooltip("A target must be within this angle of the screen center to be assisted.")]
        [SerializeField] private float _assistAngle = 5f;

        private ReticleState _publishedState = ReticleState.Hidden;
        private readonly Collider[] _assistHits = new Collider[16];

        // Lookups are cached per collider so nothing calls GetComponent every frame
        private readonly Dictionary<Collider, ITargetable> _targetCache = new Dictionary<Collider, ITargetable>();

        public bool IsAiming { get; private set; }
        public AimResult Current { get; private set; }
        // The target the camera should be gently pulled toward, or null
        public ITargetable AssistTarget { get; private set; }

        public void BeginAim()
        {
            IsAiming = true;
            if (_slowMotion != null)
                _slowMotion.Request(this, _slowMotionScale, _slowMotionDuration);
            Refresh();
        }

        public void EndAim()
        {
            IsAiming = false;
            if (_slowMotion != null)
                _slowMotion.Release(this);
            Current = default;
            AssistTarget = null;
            Publish(ReticleState.Hidden);
        }

        private void Update()
        {
            if (IsAiming)
                Refresh();
        }

        private void Refresh()
        {
            Current = Evaluate();
            AssistTarget = FindAssistTarget();
            Publish(Current.State);
        }

        // The throwable target closest to the screen center, if it is within the assist cone
        private ITargetable FindAssistTarget()
        {
            Vector3 origin = _throwOrigin.position;
            Vector3 viewPosition = _viewTransform.position;
            Vector3 viewForward = _viewTransform.forward;

            int count = Physics.OverlapSphereNonAlloc(
                origin, _range, _assistHits, _assistMask, QueryTriggerInteraction.Ignore);

            ITargetable best = null;
            float bestAngle = _assistAngle;

            for (int i = 0; i < count; i++)
            {
                ITargetable candidate = FindTarget(_assistHits[i]);
                if (candidate == null || !candidate.IsAlive)
                    continue;

                Vector3 aimPoint = candidate.AimPoint.position;
                float angle = Vector3.Angle(viewForward, aimPoint - viewPosition);
                if (angle > bestAngle)
                    continue;

                if (Physics.Linecast(origin, aimPoint, _obstacleMask, QueryTriggerInteraction.Ignore))
                    continue;

                bestAngle = angle;
                best = candidate;
            }

            return best;
        }

        public AimResult Evaluate()
        {
            var ray = new Ray(_viewTransform.position, _viewTransform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, _maxRayDistance, _aimMask, QueryTriggerInteraction.Ignore))
                return new AimResult(ReticleState.Neutral, null, ray.GetPoint(_maxRayDistance));

            ITargetable target = FindTarget(hit.collider);
            if (target == null || !target.IsAlive)
                return new AimResult(ReticleState.Neutral, null, hit.point);

            Vector3 origin = _throwOrigin.position;
            Vector3 aimPoint = target.AimPoint.position;
            bool inRange = (aimPoint - origin).sqrMagnitude <= _range * _range;
            bool blocked = Physics.Linecast(origin, aimPoint, _obstacleMask, QueryTriggerInteraction.Ignore);
            if (!inRange || blocked)
                return new AimResult(ReticleState.Neutral, null, hit.point);

            // ReticleState.Breached comes with Integrity and the Breach (phase 09)
            return new AimResult(ReticleState.Enemy, target, hit.point);
        }

        private ITargetable FindTarget(Collider hitCollider)
        {
            if (!_targetCache.TryGetValue(hitCollider, out ITargetable target))
            {
                target = hitCollider.GetComponentInParent<ITargetable>();
                _targetCache.Add(hitCollider, target);
            }

            return target;
        }

        // Only raise on change: listeners react to transitions, not to every frame
        private void Publish(ReticleState state)
        {
            if (state == _publishedState)
                return;

            _publishedState = state;
            if (_reticleChannel != null)
                _reticleChannel.Raise(state);
        }

        private void OnDrawGizmosSelected()
        {
            if (_throwOrigin == null)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_throwOrigin.position, _range);
        }
    }
}
