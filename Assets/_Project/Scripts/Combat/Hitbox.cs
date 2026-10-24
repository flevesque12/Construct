using System.Collections.Generic;
using UnityEngine;

namespace Construct.Combat
{
    // The damaging volume of an attack. While active, each Sweep() queries the Hurtbox layer
    // with a box and hits every Combatant inside, once per swing. A query instead of trigger
    // callbacks: no Rigidbody needed, and the result is known the frame we ask.
    public sealed class Hitbox : MonoBehaviour
    {
        [Tooltip("Who is attacking: its team is never hit, and it is the HitInfo source.")]
        [SerializeField] private Combatant _owner;
        [SerializeField] private LayerMask _hurtboxMask;
        [Tooltip("Box in this transform's local space.")]
        [SerializeField] private Vector3 _center = new Vector3(0f, 1f, 0.9f);
        [SerializeField] private Vector3 _size = new Vector3(1.2f, 1f, 1.4f);

        private readonly Collider[] _hits = new Collider[16];
        // One swing hits each target once, however many frames it overlaps
        private readonly HashSet<Combatant> _alreadyHit = new HashSet<Combatant>();
        private readonly Dictionary<Collider, Hurtbox> _hurtboxCache = new Dictionary<Collider, Hurtbox>();

        private AttackData _attack;

        public bool IsActive => _attack != null;

        public void Activate(AttackData attack)
        {
            _attack = attack;
            _alreadyHit.Clear();
        }

        public void Deactivate()
        {
            _attack = null;
        }

        // Returns how many new targets were hit this call
        public int Sweep()
        {
            if (_attack == null)
                return 0;

            Vector3 center = transform.TransformPoint(_center);
            int count = Physics.OverlapBoxNonAlloc(
                center, _size * 0.5f, _hits, transform.rotation, _hurtboxMask, QueryTriggerInteraction.Collide);

            int landed = 0;
            for (int i = 0; i < count; i++)
            {
                Combatant target = FindOwner(_hits[i]);
                if (target == null || target.Team == _owner.Team || !_alreadyHit.Add(target))
                    continue;

                Vector3 direction = target.transform.position - _owner.transform.position;
                direction.y = 0f;

                var hit = new HitInfo(
                    _attack.HealthDamage, _attack.IntegrityDamage,
                    _hits[i].ClosestPoint(center), direction.normalized, _owner);

                target.ReceiveHit(in hit);
                landed++;
            }

            return landed;
        }

        private Combatant FindOwner(Collider hitCollider)
        {
            if (!_hurtboxCache.TryGetValue(hitCollider, out Hurtbox hurtbox))
            {
                hurtbox = hitCollider.GetComponent<Hurtbox>();
                _hurtboxCache.Add(hitCollider, hurtbox);
            }

            return hurtbox != null ? hurtbox.Owner : null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = IsActive ? new Color(1f, 0.2f, 0.2f, 0.9f) : new Color(1f, 0.6f, 0f, 0.25f);
            Gizmos.DrawWireCube(_center, _size);
        }
    }
}
