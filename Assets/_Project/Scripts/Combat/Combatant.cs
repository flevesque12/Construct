using System;
using UnityEngine;

namespace Construct.Combat
{
    // The entry point of every hit on an entity: it splits the damage between Health and
    // Integrity (if the entity has one). Add it with a Health and some Hurtboxes, and anything
    // can be hit: player, enemy, boss, dummy, without new damage code.
    [RequireComponent(typeof(Health))]
    public sealed class Combatant : MonoBehaviour, IDamageable
    {
        [SerializeField] private CombatTeam _team = CombatTeam.Enemy;

        public CombatTeam Team => _team;
        public Health Health { get; private set; }
        public Integrity Integrity { get; private set; } // null on entities without a gauge (the player)

        // Raised after the damage is applied; feedback (flash, hitstop, sound) listens here
        public event Action<HitInfo> Hit;

        private void Awake()
        {
            Health = GetComponent<Health>();
            Integrity = GetComponent<Integrity>();
        }

        public void ReceiveHit(in HitInfo hit)
        {
            if (Health.IsDead)
                return;

            Health.TakeDamage(hit.HealthDamage);
            if (Integrity != null)
                Integrity.Add(hit.IntegrityDamage);

            Hit?.Invoke(hit);
        }
    }
}
