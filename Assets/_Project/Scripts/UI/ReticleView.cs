using Construct.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Construct.UI
{
    // Center-screen aim reticle. Only listens to the reticle channel: it never knows the player or the AimSystem.
    [RequireComponent(typeof(Image))]
    public sealed class ReticleView : MonoBehaviour
    {
        [SerializeField] private ReticleStateEventChannel _channel;

        [Header("Colors")]
        [SerializeField] private Color _neutralColor = Color.white;
        [SerializeField] private Color _enemyColor = new Color(0f, 0.85f, 1f);
        [SerializeField] private Color _breachedColor = new Color(1f, 0.2f, 0.75f);

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
            Show(ReticleState.Hidden);
        }

        private void OnEnable()
        {
            _channel.Raised += Show;
        }

        private void OnDisable()
        {
            _channel.Raised -= Show;
        }

        private void Show(ReticleState state)
        {
            _image.enabled = state != ReticleState.Hidden;

            switch (state)
            {
                case ReticleState.Neutral: _image.color = _neutralColor; break;
                case ReticleState.Enemy: _image.color = _enemyColor; break;
                case ReticleState.Breached: _image.color = _breachedColor; break;
            }
        }
    }
}
