using System;
using UnityEngine;

namespace Camera
{
    [Serializable]
    public struct CameraDraggerConfig
    {
        [SerializeField] private float dragSpeed;
        [Min(0)] [SerializeField] private float paddingRange;

        public float DragSpeed => dragSpeed;
        public float PaddingRange => paddingRange;
    }
}