using UnityEngine;

namespace Construct.Core
{
    // Turns hitstop requests from the channel into a short time freeze through SlowMotion.
    // Lives on _Systems next to SlowMotion. Hitters raise the channel instead of calling this:
    // they are often prefabs, and a prefab can reference an asset but not a scene object.
    public sealed class Hitstop : MonoBehaviour
    {
        [SerializeField] private SlowMotion _slowMotion;
        [Tooltip("Raised with a duration in real seconds.")]
        [SerializeField] private FloatEventChannel _channel;

        private float _endTime;

        private void OnEnable()
        {
            _channel.Raised += OnHitstopRequested;
        }

        private void OnDisable()
        {
            _channel.Raised -= OnHitstopRequested;
            _slowMotion.Release(this);
        }

        private void OnHitstopRequested(float duration)
        {
            // Several hits in a row: keep the longest freeze, a short one never cuts a long one
            float endTime = Mathf.Max(_endTime, Time.unscaledTime + duration);
            _endTime = endTime;
            _slowMotion.Request(this, 0f, endTime - Time.unscaledTime);
        }
    }
}
