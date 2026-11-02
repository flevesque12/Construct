using UnityEngine;
using UnityEngine.Pool;

namespace Construct.Combat
{
    // Spawns the "pixel" burst on every hit raised on the hit channel. Effects come from an
    // ObjectPool: they are created once, reused forever, so a fight never allocates
    // or destroys GameObjects mid-combat. Lives on _Systems.
    public sealed class HitVfxSpawner : MonoBehaviour
    {
        [SerializeField] private HitEventChannel _channel;
        [SerializeField] private PooledParticle _prefab;
        [Tooltip("Effects created up front, so the first hits don't instantiate anything.")]
        [SerializeField] private int _prewarm = 8;
        [Tooltip("Above this, extra effects are destroyed when released instead of kept.")]
        [SerializeField] private int _maxSize = 32;

        private ObjectPool<PooledParticle> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<PooledParticle>(
                createFunc: Create,
                actionOnGet: effect => effect.gameObject.SetActive(true),
                actionOnRelease: effect => effect.gameObject.SetActive(false),
                actionOnDestroy: effect => Destroy(effect.gameObject),
                collectionCheck: false,
                defaultCapacity: _prewarm,
                maxSize: _maxSize);

            // Get them all, then give them back: the pool now holds ready-made effects
            var warm = new PooledParticle[_prewarm];
            for (int i = 0; i < _prewarm; i++)
                warm[i] = _pool.Get();
            for (int i = 0; i < _prewarm; i++)
                _pool.Release(warm[i]);
        }

        private void OnEnable()
        {
            _channel.Raised += OnHit;
        }

        private void OnDisable()
        {
            _channel.Raised -= OnHit;
        }

        private void OnHit(HitInfo hit)
        {
            // The hit point is on the struck surface, facing the attacker: spray out of that
            // surface (back toward the attacker), or the burst would vanish inside the body
            Quaternion rotation = hit.Direction.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(-hit.Direction)
                : Quaternion.identity;

            _pool.Get().Play(hit.Point, rotation);
        }

        private PooledParticle Create()
        {
            PooledParticle effect = Instantiate(_prefab, transform);
            effect.SetPool(_pool);
            return effect;
        }
    }
}
