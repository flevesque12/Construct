using UnityEngine;

namespace Construct.Combat
{
    // One attack, as data: which animation, how fast, and when it can chain or be cancelled.
    // Windows are normalized (0 = start of the attack, 1 = end) so they survive speed changes.
    // Timing lives here instead of Animation Events because the clips belong to a third-party
    // pack (git-ignored): events written into its import settings would not survive a clone.
    [CreateAssetMenu(fileName = "Attack", menuName = "Construct/Combat/Attack Data")]
    public sealed class AttackData : ScriptableObject
    {
        [Tooltip("State name in the player's Animator Controller.")]
        [SerializeField] private string _animatorState;
        [Tooltip("The clip that state plays, used to know how long the attack lasts.")]
        [SerializeField] private AnimationClip _clip;
        [Min(0.1f)]
        [SerializeField] private float _speed = 1f;
        [Tooltip("Cross-fade into this attack, in seconds.")]
        [SerializeField] private float _transitionDuration = 0.08f;

        [Header("Windows (normalized)")]
        [Tooltip("A buffered attack press during this window chains into the next hit.")]
        [SerializeField] private Vector2 _comboWindow = new Vector2(0.35f, 0.8f);
        [Tooltip("From here on, moving (and dodging, later) interrupts the attack.")]
        [Range(0f, 1f)]
        [SerializeField] private float _cancelStart = 0.7f;

        private int _stateHash;

        public int StateHash => _stateHash;
        public float Speed => _speed;
        public float TransitionDuration => _transitionDuration;
        public float Duration => _clip != null ? _clip.length / _speed : 0.5f;
        public float CancelStart => _cancelStart;

        public bool IsInComboWindow(float normalizedTime)
        {
            return normalizedTime >= _comboWindow.x && normalizedTime <= _comboWindow.y;
        }

        private void OnEnable()
        {
            // Hashed once, so playing an attack never hashes a string at runtime
            _stateHash = Animator.StringToHash(_animatorState);
        }

        private void OnValidate()
        {
            _stateHash = Animator.StringToHash(_animatorState);
            _comboWindow.x = Mathf.Clamp01(_comboWindow.x);
            _comboWindow.y = Mathf.Clamp(_comboWindow.y, _comboWindow.x, 1f);
        }
    }
}
