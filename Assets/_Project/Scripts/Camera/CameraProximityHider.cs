using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;

namespace Construct.Cameras
{
    // Switches the target's renderers to shadows-only when the camera gets
    // pushed too close (e.g. backed against a wall), so it never renders inside it.
    public sealed class CameraProximityHider : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Renderer[] _renderers;
        [Tooltip("Camera-to-target distance below which the renderers are hidden.")]
        [SerializeField] private float _hideDistance = 1f;

        private ShadowCastingMode[] _defaultModes;
        private bool _hidden;

        private void Awake()
        {
            _defaultModes = new ShadowCastingMode[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _defaultModes[i] = _renderers[i].shadowCastingMode;
        }

        private void OnEnable()
        {
            // Runs after the brain has placed the camera for this frame
            CinemachineCore.CameraUpdatedEvent.AddListener(OnCameraUpdated);
        }

        private void OnDisable()
        {
            CinemachineCore.CameraUpdatedEvent.RemoveListener(OnCameraUpdated);
            SetHidden(false);
        }

        private void OnCameraUpdated(CinemachineBrain brain)
        {
            Vector3 cameraPosition = brain.OutputCamera.transform.position;
            float sqrDistance = (cameraPosition - _target.position).sqrMagnitude;
            SetHidden(sqrDistance < _hideDistance * _hideDistance);
        }

        private void SetHidden(bool hidden)
        {
            if (_hidden == hidden)
                return;

            _hidden = hidden;
            for (int i = 0; i < _renderers.Length; i++)
                _renderers[i].shadowCastingMode = hidden ? ShadowCastingMode.ShadowsOnly : _defaultModes[i];
        }
    }
}
