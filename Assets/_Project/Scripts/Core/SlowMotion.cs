using System.Collections.Generic;
using UnityEngine;

namespace Construct.Core
{
    // The only writer of Time.timeScale. Several systems can ask for slow motion at once
    // (aiming, Perfect Dodge, hitstop later): the slowest request wins, and time goes back
    // to normal when the last one ends. Lives on _Systems.
    public sealed class SlowMotion : MonoBehaviour
    {
        private struct Entry
        {
            public object Owner;
            public float Scale;
            public float EndTime; // unscaled; 0 = until released
        }

        private readonly List<Entry> _entries = new List<Entry>(8);
        private float _baseFixedDeltaTime;

        private void Awake()
        {
            _baseFixedDeltaTime = Time.fixedDeltaTime;
        }

        // duration is in real seconds; 0 or less lasts until Release
        public void Request(object owner, float scale, float duration)
        {
            Remove(owner);
            float endTime = duration > 0f ? Time.unscaledTime + duration : 0f;
            _entries.Add(new Entry { Owner = owner, Scale = Mathf.Max(0f, scale), EndTime = endTime });
            Apply();
        }

        public void Release(object owner)
        {
            if (Remove(owner))
                Apply();
        }

        private void Update()
        {
            bool expired = false;
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                float endTime = _entries[i].EndTime;
                if (endTime > 0f && Time.unscaledTime >= endTime)
                {
                    _entries.RemoveAt(i);
                    expired = true;
                }
            }

            if (expired)
                Apply();
        }

        private void OnDisable()
        {
            _entries.Clear();
            Apply();
        }

        private bool Remove(object owner)
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Owner == owner)
                {
                    _entries.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private void Apply()
        {
            float scale = 1f;
            foreach (Entry entry in _entries)
                scale = Mathf.Min(scale, entry.Scale);

            Time.timeScale = scale;
            // Physics steps shrink with time so slowed motion stays smooth (never 0, Unity rejects it)
            Time.fixedDeltaTime = _baseFixedDeltaTime * Mathf.Max(scale, 0.01f);
        }
    }
}
