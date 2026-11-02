using Construct.Core;
using UnityEngine;

namespace Construct.Combat
{
    // Makes a hit on this entity felt: a white flash on its renderers, and requests for
    // hitstop and camera shake through event channels (time and camera belong to other systems).
    // A Breach gets its own, bigger punch.
    [RequireComponent(typeof(Combatant))]
    public sealed class HitFeedback : MonoBehaviour
    {
        [Header("Flash")]
        [SerializeField] private Renderer[] _renderers = new Renderer[0];
        [Tooltip("Swapped in for a moment on every hit (unlit white).")]
        [SerializeField] private Material _flashMaterial;
        [Tooltip("Real seconds, so the flash is still visible during the hitstop freeze.")]
        [SerializeField] private float _flashDuration = 0.08f;

        [Header("Channels")]
        [SerializeField] private FloatEventChannel _hitstopChannel;
        [SerializeField] private FloatEventChannel _cameraImpulseChannel;
        [Tooltip("Global hit event: the pixel particles (and audio later) react to it.")]
        [SerializeField] private HitEventChannel _hitChannel;

        [Header("Breach")]
        [SerializeField] private float _breachHitstop = 0.12f;
        [SerializeField] private float _breachImpulse = 1f;

        private Combatant _combatant;
        private Integrity _integrity;
        private Material[][] _originalMaterials;
        private Material[][] _flashMaterials;
        private float _flashEndTime;
        private bool _isFlashing;

        private void Awake()
        {
            _combatant = GetComponent<Combatant>();
            _integrity = GetComponent<Integrity>();

            // Both material sets are built once, so a flash never allocates
            _originalMaterials = new Material[_renderers.Length][];
            _flashMaterials = new Material[_renderers.Length][];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _originalMaterials[i] = _renderers[i].sharedMaterials;
                _flashMaterials[i] = new Material[_originalMaterials[i].Length];
                for (int m = 0; m < _flashMaterials[i].Length; m++)
                    _flashMaterials[i][m] = _flashMaterial;
            }
        }

        private void OnEnable()
        {
            _combatant.Hit += OnHit;
            if (_integrity != null)
                _integrity.Breached += OnBreached;
        }

        private void OnDisable()
        {
            _combatant.Hit -= OnHit;
            if (_integrity != null)
                _integrity.Breached -= OnBreached;
            SetFlash(false);
        }

        private void Update()
        {
            if (_isFlashing && Time.unscaledTime >= _flashEndTime)
                SetFlash(false);
        }

        private void OnHit(HitInfo hit)
        {
            _flashEndTime = Time.unscaledTime + _flashDuration;
            SetFlash(true);
            Request(_hitstopChannel, hit.Hitstop);
            Request(_cameraImpulseChannel, hit.Impulse);
            if (_hitChannel != null)
                _hitChannel.Raise(hit);
        }

        private void OnBreached()
        {
            Request(_hitstopChannel, _breachHitstop);
            Request(_cameraImpulseChannel, _breachImpulse);
        }

        private static void Request(FloatEventChannel channel, float value)
        {
            if (channel != null && value > 0f)
                channel.Raise(value);
        }

        private void SetFlash(bool on)
        {
            if (_isFlashing == on || _flashMaterial == null)
                return;

            _isFlashing = on;
            Material[][] set = on ? _flashMaterials : _originalMaterials;
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].sharedMaterials = set[i];
        }
    }
}
