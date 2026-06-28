using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace FTL
{
    [System.Serializable]
    public class Room
    {
        public bool HasFacility => FacilitySystem != null;
        public bool IsUnintialized() => Id == 0 && MaxCrewCapacity == 0;
        [field: SerializeField] private List<int> neighborRoomIds;
        [field: SerializeField] public FacilitySystem FacilitySystem { get; set; }
        [field: SerializeField] public List<CrewController> Crews { get; set; } = new List<CrewController>();
        [field: SerializeField] public ERoomType Type { get; private set; }
        [field: SerializeField] public Vector2 LocalPosition { get; set; }
        [field: SerializeField] public Vector2 GlobalPosition { get; set; }
        [field: SerializeField] public int MaxCrewCapacity { get; private set; }
        [field: SerializeField] public int Id { get; private set; }

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
            if (Crews.Contains(crew))
            {
                Debug.LogWarning($"Crew {crew.name} is already in Room {Id}. Cannot add again.");
                return;
            }

            ;
            if (Crews.Count == MaxCrewCapacity)
            {
                Debug.LogWarning($"Room {Id} is at full capacity. Cannot add Crew {crew.name}.");
                return;
            }

            Crews.Add(crew);
        }
        public void ReduceCrew(CrewController crew)
        {
            if (Crews.Contains(crew) == false)
            {
                Debug.LogWarning($"Crew {crew.name} is not in Room {Id}. Cannot remove.");
                return;
            }

            Crews.Remove(crew);
        }

        public void RepairFacility()
        {
            float repairDuration = FacilitySystem.RepairDuration;
            float newRepairProgress = FacilitySystem.RepairProgress;
            newRepairProgress = Mathf.Min(repairDuration, newRepairProgress + Time.deltaTime);

            if (newRepairProgress == repairDuration)
            {
                FacilitySystem.Repair(1);
                newRepairProgress = 0f;
            }

            FacilitySystem.RepairProgress = newRepairProgress;
        }

        public void ApplyAssistEffectOnFacility(bool isApplied)
        {
            FacilitySystem.ApplyAssistEffectOnFacility(isApplied);
        }

        public void TakeDamageToCrews(int crewDamage)
        {
            for (int i = 0; i < Crews.Count; i++)
            {
                Crews[i].TakeDamage(crewDamage);
            }
        }
    }
}
