using System;
using UnityEngine;

namespace FTL
{
    public class CrewController : MonoBehaviour
    {
        public bool IsDead => Hp == 0;
        [field: SerializeField] public bool IsSelected { get; set; } = false;
        [field: SerializeField] public Room CurrentRoom { get; set; } = null;
        [field: SerializeField] public Room TargetRoom { get; private set; } = null;
        [field: SerializeField] public int MaxHp { get; set; } = 100;
        [field: SerializeField] public int Hp { get; set; } = 0;
        [field: SerializeField] public string Name { get; set; } = "Hans";
        public CrewSystem CrewSystem { set; get; }
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private ECrewStatus crewStatus = ECrewStatus.Idle;
        [field: SerializeField] public static Vector3 Offset { get; private set; } = new Vector3(0f, 0.5f, 0f);
        private Transform crewSpriteTransform;

        #region Unity Method

        private void Awake()
        {
            Hp = MaxHp;
            crewSpriteTransform = transform.GetChild(0).transform;
            CurrentRoom = null;
            TargetRoom = null;
        }

        private void Update()
        {
            if (CrewSystem == null || CrewSystem.CanRunCrewLogic() == false)
            {
                return;
            }

            UpdateCrewStatus();
            HandleStatus();
        }

        #endregion

        #region Public Method

        public void SetTargetRoom(Room room)
        {
            TargetRoom = room;
            CurrentRoom.ReduceCrew(this);
            // 이동하기전 보조 정지하고 가야함
            if (CurrentRoom.IsEmptyCrew())
            {
                AssistFacility(false);
                ResetRepairProgressOnEmptyRoom(CurrentRoom);
            }

            TargetRoom.AddCrew(this);
            CurrentRoom = TargetRoom;
            crewStatus = ECrewStatus.Moving;
        }

        public void InitStartRoom(Room room)
        {
            CurrentRoom = room;
            room.AddCrew(this);
            crewSpriteTransform.position = room.GlobalPosition;
        }

        public void ApplyCrewState(CrewState crewState, Room room)
        {
            CurrentRoom.ReduceCrew(this);
            InitStartRoom(room);
            Hp = crewState.Hp;
            Name = crewState.Name;
        }

        public void HealHp(int amount)
        {
            Hp = Mathf.Min(MaxHp, Hp + amount);
        }

        public void TakeDamage(int amount)
        {
            Hp = Mathf.Max(0, Hp - amount);
            if (IsDead)
            {
                //CurrentRoom.ReduceCrew(this);
                //if (CurrentRoom != TargetRoom)
                //    TargetRoom.ReduceCrew(this);
                CrewSystem.HandleCrewDeath(this);
                gameObject.SetActive(false);
            }
        }

        #endregion

        #region Private Method

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
                switch (CurrentRoom.GetFacilityStatus())
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
                        Debug.LogWarning($"CrewController: UpdateStatus: Unexpected facility status {CurrentRoom.GetFacilityStatus()} in room {CurrentRoom.Id}");
                        break;
                }
            }
        }

        private bool CanMoveToLowPosition()
        {
            Vector3 roomAvailablePosition = CurrentRoom.GetCrewPosition(this, Offset);
            bool canMoveToLowPosition = roomAvailablePosition != crewSpriteTransform.position;
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
                Vector3 targetPosition = TargetRoom.GetCrewPosition(this, Offset);
                bool hasReachedTarget = crewSpriteTransform.position == targetPosition;
                if (hasReachedTarget)
                {
                    crewStatus = ECrewStatus.Idle;
                    TargetRoom = null;
                }
                else
                {
                    float moveDistance = moveSpeed * Time.deltaTime;
                    crewSpriteTransform.position = Vector3.MoveTowards(crewSpriteTransform.position, targetPosition, moveDistance);
                }
            }
        }

        private void HandleStatus()
        {
            switch (crewStatus)
            {
                case ECrewStatus.Moving:
                    Room roomBeforeMoving = CurrentRoom;
                    MoveToTargetRoom();
                    break;
                case ECrewStatus.Idle:
                    AssistFacility(false);
                    ResetRepairProgressOnEmptyRoom(CurrentRoom);
                    break;
                case ECrewStatus.Repair:
                    AssistFacility(false);
                    RepairFacilityInRoom();
                    break;
                case ECrewStatus.Assisting:
                    AssistFacility(true);
                    ResetRepairProgressOnEmptyRoom(CurrentRoom);
                    break;
                default:
                    break;
            }
        }

        private static void ResetRepairProgressOnEmptyRoom(Room room)
        {
            if (room.IsEmptyCrew() && room.HasFacility)
            {
                room.ResetRepairProgress();
            }
        }

        #endregion
    }
}
