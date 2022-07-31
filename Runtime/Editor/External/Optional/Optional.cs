// https://gist.github.com/aarthificial/f2dbb58e4dbafd0a93713a380b9612af

using System;
using UnityEngine;

namespace Boxsubmus.Editor
{
    [Serializable]
    /// Requires Unity 2020.1+
    public struct Optional<T>
    {
        [SerializeField] private bool enabled;
        [SerializeField] private T value;

        public bool Enabled { get { return enabled; } set { enabled = value; } }
        public T Value => value;

        public Optional(T initialValue)
        {
            enabled = true;
            value = initialValue;
        }
    }
}