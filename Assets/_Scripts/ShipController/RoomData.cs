using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    [System.Serializable]
    public class RoomData
    {
        public bool HasFacility => FacilitySystem != null;
        public void SetRoomData(RoomData roomData)
        {
            Id = roomData.Id;
            Type = roomData.Type;
            LocalPosition = roomData.LocalPosition;
            GlobalPosition = roomData.GlobalPosition;
            neighborRoomIds = new List<int>(roomData.neighborRoomIds);
        }

        [field: SerializeField] public int Id { get; private set; }
        [field: SerializeField] public ERoomType Type { get; private set; }
        [field: SerializeField] public Vector2 LocalPosition { get; set; }
        [field: SerializeField] public Vector2 GlobalPosition { get; set; }
        [field: SerializeField] public FacilitySystem FacilitySystem { get; set; }

        [field: SerializeField] private List<int> neighborRoomIds;
    }
}
