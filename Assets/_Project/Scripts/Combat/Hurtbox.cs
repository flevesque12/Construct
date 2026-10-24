using UnityEngine;

namespace Construct.Combat
{
    // A collider that can be hit, on the Hurtbox layer. It only points to its owner:
    // an entity can have several (head, body) that all feed the same Combatant.
    [RequireComponent(typeof(Collider))]
    public sealed class Hurtbox : MonoBehaviour
    {
        public Combatant Owner { get; private set; }

        private void Awake()
        {
            Owner = GetComponentInParent<Combatant>();
        }
    }
}
