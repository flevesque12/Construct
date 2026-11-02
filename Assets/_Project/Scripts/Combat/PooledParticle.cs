using UnityEngine;
using UnityEngine.Pool;

namespace Construct.Combat
{
    // A particle effect that goes back to its pool by itself when it finishes.
    // The ParticleSystem's Stop Action must be "Callback" for OnParticleSystemStopped to fire.
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class PooledParticle : MonoBehaviour
    {
        private ParticleSystem _particles;
        private IObjectPool<PooledParticle> _pool;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        public void SetPool(IObjectPool<PooledParticle> pool)
        {
            _pool = pool;
        }

        public void Play(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            _particles.Play();
        }

        private void OnParticleSystemStopped()
        {
            _pool.Release(this);
        }
    }
}
