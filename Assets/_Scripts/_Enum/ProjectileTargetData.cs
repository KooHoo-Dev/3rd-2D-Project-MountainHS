using System;
using UnityEngine;

namespace FTL

{
    [Serializable]
    public struct ProjectileTargetData
    {
        public Vector3 TargetPosition;
        public int RoomId;

        public ProjectileTargetData(Vector3 targetPosition, int roomId)
        {
            TargetPosition = targetPosition;
            RoomId = roomId;
        }

        public void Reset()
        {
            TargetPosition = Vector3.zero;
            RoomId = -1;
        }

        public bool IsValid()
        {
            return TargetPosition != Vector3.zero && RoomId >= 0;
        }
    }
}
