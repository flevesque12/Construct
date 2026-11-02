using Construct.Core;
using UnityEngine;

namespace Construct.Combat
{
    // Every landed hit, for whoever wants to react globally (VFX now, audio later).
    // In Combat rather than Core because its payload, HitInfo, is a combat type.
    [CreateAssetMenu(fileName = "HitChannel", menuName = "Construct/Events/Hit Channel")]
    public sealed class HitEventChannel : EventChannel<HitInfo> { }
}
