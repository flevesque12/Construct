using Construct.Core;
using UnityEngine;

namespace Construct.Weapon
{
    // What a throw would do right now. A struct: evaluated every frame while aiming, never allocated.
    public readonly struct AimResult
    {
        public readonly ReticleState State;
        public readonly ITargetable Target; // null unless an enemy is in range
        public readonly Vector3 Point;      // where the camera ray lands (or its end if nothing is hit)

        public AimResult(ReticleState state, ITargetable target, Vector3 point)
        {
            State = state;
            Target = target;
            Point = point;
        }
    }
}
