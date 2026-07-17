using System;
using UnityEngine;

namespace FTL
{
    [Serializable]
    public class WeaponTransformData
    {
        [field: SerializeField] public Vector3 WeaponOffset { get; private set; }
        [field: SerializeField] public Vector3 WeaponRotation { get; private set; }
        [field: SerializeField] public bool isFlipX;
        [field: SerializeField] public bool isFlipY;
    }
}
