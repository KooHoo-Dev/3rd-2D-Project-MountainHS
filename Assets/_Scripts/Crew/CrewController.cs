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
        [SerializeField] private ECrewStatus crewStatus = ECrewStatus.Idle;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0.5f, 0f);
        [SerializeField] private float moveSpeed = 3f;

        private void Update()
        {
            UpdateCrewStatus();
            HandleStatus();
        }

        public void SetTargetRoom(Room room)
        {
            TargetRoom = room;
            CurrentRoom.ReduceCrew(this);
            // 이동하기전 보조 정지하고 가야함
            if (CurrentRoom.CrewList.Count == 0)
            {
                AssistFacility(false);
            }

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

        private void UpdateCrewStatus()
        {
            if (crewStatus == ECrewStatus.Moving)
            {
                return;
            }

            if (CanMoveToLowPosition())
            {
                crewStatus = ECrewStatus.Moving;
                TargetRoom = CurrentRoom;
                return;
            }

            if (CurrentRoom.HasFacility)
            {
                FacilitySystem facility = CurrentRoom.FacilitySystem;
                switch (facility.FacilityStatus)
                {
                    case EFacilityStatus.Damaged:
                    case EFacilityStatus.Broked:
                        crewStatus = ECrewStatus.Repair;
                        break;
                    case EFacilityStatus.Normal:
                        crewStatus = ECrewStatus.Assisting;
                        break;
                    case EFacilityStatus.Disabled:
                        crewStatus = ECrewStatus.Idle;
                        break;
                    default:
                        Debug.LogWarning($"CrewController: UpdateStatus: Unexpected facility status {facility.FacilityStatus} in room {CurrentRoom.Id}");
                        break;
                }
            }
        }

        private bool CanMoveToLowPosition()
        {
            int crewIndexInCurrentRoom = CurrentRoom.CrewList.IndexOf(this);
            Vector2 crewOffset = (Vector2)(offset * crewIndexInCurrentRoom);
            Vector3 roomAvailablePosition = CurrentRoom.GlobalPosition + crewOffset;
            bool canMoveToLowPosition = roomAvailablePosition != transform.position;
            return canMoveToLowPosition;
        }

        private void RepairFacilityInRoom()
        {
            CurrentRoom.RepairFacility();
        }

        private void AssistFacility(bool isAssist)
        {
            if (CurrentRoom.HasFacility)
                CurrentRoom.ApplyAssistEffectOnFacility(isAssist);
        }

        private void MoveToTargetRoom()
        {
            bool hasValidTargetRoom = TargetRoom?.IsUnintialized() == false;
            if (hasValidTargetRoom)
            {
                //bool hasReachedTargetX = transform.position.x == TargetRoom.GlobalPosition.x;
                int crewIndexInRoom = TargetRoom.CrewList.IndexOf(this);
                Vector2 crewOffset = (Vector2)(offset * crewIndexInRoom);
                Vector3 targetPosition = TargetRoom.GlobalPosition + crewOffset;
                bool hasReachedTarget = transform.position == targetPosition;
                if (hasReachedTarget)
                {
                    crewStatus = ECrewStatus.Idle;
                    TargetRoom = null;
                }
                else
                {
                    float moveDistance = moveSpeed * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveDistance);
                }
            }
        }

        private void HandleStatus()
        {
            switch (crewStatus)
            {
                case ECrewStatus.Moving:
                    MoveToTargetRoom();
                    break;
                case ECrewStatus.Idle:
                    AssistFacility(false);
                    break;
                case ECrewStatus.Repair:
                    AssistFacility(false);
                    RepairFacilityInRoom();
                    break;
                case ECrewStatus.Assisting:
                    AssistFacility(true);
                    break;
                default:
                    break;
            }
        }

    }
}
