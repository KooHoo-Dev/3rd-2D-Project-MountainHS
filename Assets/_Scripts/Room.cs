using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    [System.Serializable]
    public class Room
    {
        [field: SerializeField] public int RoomId { get; private set; }
        [field: SerializeField] public ERoomType RoomType { get; private set; }
        [field: SerializeField] private List<int> neighborRoomIds;
    }
}
