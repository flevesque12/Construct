using Construct.Core;
using UnityEngine;

namespace Construct.Weapon
{
    // Answers "what would a throw do right now?" while the player aims, and tells the HUD
    // through the reticle channel. It never reads input: AimState calls BeginAim / EndAim.
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

        private ReticleState _publishedState = ReticleState.Hidden;

        // The ray usually stays on the same collider for many frames: cache its lookup
        private Collider _lastCollider;
        private ITargetable _lastTarget;

        public bool IsAiming { get; private set; }
        public AimResult Current { get; private set; }

        public void BeginAim()
        {
            IsAiming = true;
            Refresh();
        }

        public void EndAim()
        {
            IsAiming = false;
            Current = default;
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
            Publish(Current.State);
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
            if (hitCollider != _lastCollider)
            {
                _lastCollider = hitCollider;
                _lastTarget = hitCollider.GetComponentInParent<ITargetable>();
            }

            return _lastTarget;
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
