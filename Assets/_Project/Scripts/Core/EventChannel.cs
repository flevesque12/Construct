using System;
using UnityEngine;

namespace Construct.Core
{
    // A ScriptableObject that carries one kind of event between systems that don't
    // know each other: gameplay raises it, UI / audio / VFX listen to the same asset.
    // Unity can't create assets of a generic type, so each payload gets a tiny subclass.
    public abstract class EventChannel<T> : ScriptableObject
    {
        public event Action<T> Raised;

        public void Raise(T value)
        {
            Raised?.Invoke(value);
        }
    }
}
