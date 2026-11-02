using Construct.Core;
using Unity.Cinemachine;
using UnityEngine;

namespace Construct.Cameras
{
    // Plays a Cinemachine impulse (camera shake) whenever gameplay raises the impulse channel.
    // Gameplay only knows the channel; this is where it turns into Cinemachine.
    // Every CinemachineCamera needs a CinemachineImpulseListener extension to feel it.
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public sealed class CameraImpulsePlayer : MonoBehaviour
    {
        [Tooltip("Raised with a force; 1 is a solid heavy hit.")]
        [SerializeField] private FloatEventChannel _channel;

        private CinemachineImpulseSource _source;

        private void Awake()
        {
            _source = GetComponent<CinemachineImpulseSource>();
            // Shake in real time: a hit raises a hitstop (time scale 0) at the same moment,
            // and the shake must play during the freeze, not after it
            CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        }

        private void OnEnable()
        {
            _channel.Raised += _source.GenerateImpulseWithForce;
        }

        private void OnDisable()
        {
            _channel.Raised -= _source.GenerateImpulseWithForce;
        }
    }
}
