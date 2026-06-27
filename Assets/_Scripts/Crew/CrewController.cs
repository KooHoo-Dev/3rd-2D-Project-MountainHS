using System;
using UnityEngine;

namespace FTL
{
    public class CrewController : MonoBehaviour
    {
        [field: SerializeField] public Room CurrentRoom { get; set; } = null;
        [field: SerializeField] public Room TargetRoom { get; private set; } = null;
        public int Hp { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float repairDuration = 7.5f;
        [SerializeField] private float repairProgress = 0f;
        private ECrewStatus crewStatus = ECrewStatus.Idle;


        private void Update()
        {
            MoveToTargetRoom();
            UpdateCrewStatus();
        }

        public void MoveToTargetRoom()
        {
            if (TargetRoom?.IsUnintialized() == false)
            {
                if ((Vector2)transform.position == TargetRoom.GlobalPosition)
                {
                    crewStatus = ECrewStatus.Idle;
                    TargetRoom = null;
                }
                else
                {
                    Vector3 targetPosition = TargetRoom.GlobalPosition + (Vector2)(offset * (TargetRoom.CrewList.IndexOf(this)));
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                }
            }
        }

        public void SetTargetRoom(Room room)
        {
            TargetRoom = room;
            CurrentRoom.ReduceCrew(this);
            TargetRoom.AddCrew(this);
            CurrentRoom = TargetRoom;
            crewStatus = ECrewStatus.Moving;
        }

        public void InitStartRoom(Room room)
        {
            CurrentRoom = room;
            room.AddCrew(this);
            transform.position = room.GlobalPosition;
        }

        public void UpdateCrewStatus()
        {
            if (crewStatus == ECrewStatus.Idle)
            {
                FacilitySystem facility = CurrentRoom.FacilitySystem;
                if (facility != null)
                {
                    switch (facility.FacilityStatus)
                    {
                        case EFacilityStatus.Damaged:
                        case EFacilityStatus.Broked:
                            crewStatus = ECrewStatus.Repair;
                            RepairFacility();
                            break;
                        case EFacilityStatus.Normal:
                            crewStatus = ECrewStatus.Assisting;
                            AssistFacility();
                            // 시설 보조 로직
                            break;
                        default:
                            Debug.LogWarning($"CrewController: UpdateStatus: Unexpected facility status {facility.FacilityStatus} in room {CurrentRoom.Id}");
                            break;
                    }
                }
            }
        }

        private void RepairFacility()
        {
            
        }

        private void AssistFacility()
        {

        }
    }
}
