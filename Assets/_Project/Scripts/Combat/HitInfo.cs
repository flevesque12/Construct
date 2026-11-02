using UnityEngine;

namespace Construct.Combat
{
    // Everything a target needs to know about one hit. A readonly struct passed with `in`:
    // no allocation and no copy per hit, and nobody down the chain can change it.
    public readonly struct HitInfo
    {
        public readonly float HealthDamage;
        public readonly float IntegrityDamage;
        public readonly Vector3 Point;      // where it connected, for VFX
        public readonly Vector3 Direction;  // attacker to target, flattened, for knockback
        public readonly Combatant Source;

        // How the hit should feel, from the attack's data
        public readonly float Hitstop;      // real seconds of frozen time
        public readonly float Impulse;      // camera shake force, 0 = none

        public HitInfo(float healthDamage, float integrityDamage, Vector3 point, Vector3 direction,
            Combatant source, float hitstop, float impulse)
        {
            HealthDamage = healthDamage;
            IntegrityDamage = integrityDamage;
            Point = point;
            Direction = direction;
            Source = source;
            Hitstop = hitstop;
            Impulse = impulse;
        }
    }
}
