using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    [System.Serializable]
    public class Room
    {
        public bool HasFacility => FacilitySystem != null;
        public bool IsUnintialized() => Id == 0 && MaxCrewCapacity == 0;
        public void SetRoomData(Room room)
        {
            Id = room.Id;
            Type = room.Type;
            LocalPosition = room.LocalPosition;
            GlobalPosition = room.GlobalPosition;
            MaxCrewCapacity = room.MaxCrewCapacity;
            neighborRoomIds = new List<int>(room.neighborRoomIds);
        }
        public void AddCrew(CrewController crew)
        {
            if (CrewList.Contains(crew))
            {
                Debug.LogWarning($"Crew {crew.name} is already in Room {Id}. Cannot add again.");
                return;
            }
            if (CrewList.Count == MaxCrewCapacity)
            {
                Debug.LogWarning($"Room {Id} is at full capacity. Cannot add Crew {crew.name}.");
                return;
            }

            CrewList.Add(crew);
        }
        public void ReduceCrew(CrewController crew)
        {
            if (CrewList.Contains(crew) == false)
            {
                Debug.LogWarning($"Crew {crew.name} is not in Room {Id}. Cannot remove.");
                return;
            }

            CrewList.Remove(crew);
        }

        [field: SerializeField] private List<int> neighborRoomIds;
        [field: SerializeField] public FacilitySystem FacilitySystem { get; set; }
        [field: SerializeField] public List<CrewController> CrewList { get; set; } = new List<CrewController>();
        [field: SerializeField] public ERoomType Type { get; private set; }
        [field: SerializeField] public Vector2 LocalPosition { get; set; }
        [field: SerializeField] public Vector2 GlobalPosition { get; set; }
        [field: SerializeField] public int MaxCrewCapacity { get; private set; }
        [field: SerializeField] public int Id { get; private set; }
    }
}
