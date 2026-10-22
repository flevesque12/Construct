using UnityEngine;

namespace Construct.Core
{
    [CreateAssetMenu(fileName = "ReticleStateChannel", menuName = "Construct/Events/Reticle State Channel")]
    public sealed class ReticleStateEventChannel : EventChannel<ReticleState> { }
}
