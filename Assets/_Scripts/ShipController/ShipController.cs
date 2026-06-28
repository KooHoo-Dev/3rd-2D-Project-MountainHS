using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace FTL
{
    [DefaultExecutionOrder(1000)]
    public class ShipController : MonoBehaviour, IDamageable, IDeadable
    {
        public bool IsDead => Hp == 0;
        [Header("Ship Data")]
        [field: SerializeField] public ShipData ShipData { get; private set; }

        [field: SerializeField] public int Hp { get; private set; }

        [Header("Power System")]
        [SerializeField] protected ShipPowerController powerSystem;

        [Header("Room")]
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private Transform roomGroupTransform;
        [field: SerializeField] public List<Room> Rooms { get; private set; } = new List<Room>();

        [Header("Crew")]
        [SerializeField] private Transform crewGroupTransform;
        [SerializeField] private GameObject crewPrefab;
        [SerializeField] protected int crewCount = 3;

        public List<CrewController> Crews { get; private set; } = new List<CrewController>();
        protected List<FacilitySystem> facilitySystems = new List<FacilitySystem>() ;
        protected CrewSystem crewSystem;
        protected WeaponSystem weaponSystem;
        protected ShipOwnership shipOwnership;

        protected virtual void Awake()
        {
            Hp = ShipData.MaxHp;
            facilitySystems.Add(GetComponent<WeaponSystem>());
            facilitySystems.Add(GetComponent<EngineSystem>());
            facilitySystems.Add(GetComponent<ShieldSystem>());
            crewSystem = GetComponent<CrewSystem>();

            shipOwnership = GetComponentInChildren<ShipOwnership>();

            weaponSystem = (WeaponSystem)GetFacilitySystemByERoomType(ERoomType.Weapon);

            InitRooms();
            InitCrews();

            void InitRooms()
            {
                for (int i = 0; i < ShipData.Rooms.Count; i++)
                {
                    GameObject roomObject = Instantiate(roomPrefab, roomGroupTransform);
                    RoomViewer roomViewer = roomObject.GetComponent<RoomViewer>();
                    Room room = new Room();
                    // RoomData 초기화 함수 생성
                    room.SetRoomData(ShipData.Rooms[i]);
                    roomObject.transform.localPosition = room.LocalPosition;
                    room.GlobalPosition = roomObject.transform.position;
                    roomViewer.Room = room;
                    roomViewer.SetRoomState();

                    if (room.Type != ERoomType.Empty)
                    {
                        room.FacilitySystem = GetFacilitySystemByERoomType(room.Type);
                    }

                    Rooms.Add(room);
                }
            }

            void InitCrews()
            {
                //CrewController[] crews = GetComponentsInChildren<CrewController>();
                for (int i = 0; i < crewCount; i++)
                {
                    // 승무원 시작 방 하드 코딩
                    //CrewController crew = crews[i];
                    //Room room = Rooms[i];
                    //Crews.Add(crew);
                    //crew.InitStartRoom(room);

                    GameObject crewObject = Instantiate(crewPrefab, crewGroupTransform);
                    CrewController crew = crewObject.GetComponent<CrewController>();
                    Room room = Rooms[i];
                    crew.InitStartRoom(room);
                    Crews.Add(crew);
                }
            }
        }

        protected void HandlePowerInput(ERoomType facilityType, bool isRecoverInput)
        {
            if (IsDead)
                return;

            if (isRecoverInput)
            {
                RequestDeallocatePower(facilityType);
            }
            else
            {
                RequestAllocatePower(facilityType);
            }
        }

        // roomId를 받으면 Rooms에서 탐색해서 해당 room 리턴
        protected Room GetRoomByIdOrNull(int roomId)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                if (Rooms[i].Id == roomId)
                    return Rooms[i];
            }
            return null;
        }

        protected Room GetRoomByPositionOrNull(Vector2 position)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                if (Rooms[i].GlobalPosition == position)
                    return Rooms[i];
            }
            return null;
        }

        public FacilitySystem GetFacilitySystemByERoomType(ERoomType roomType)
        {
            switch (roomType)
            {
                case ERoomType.Weapon:
                    return facilitySystems[(int)ERoomType.Weapon];
                case ERoomType.Engine:
                    return facilitySystems[(int)ERoomType.Engine];
                case ERoomType.Shield:
                    return facilitySystems[(int)ERoomType.Shield];
                default:
                    throw new ArgumentException("Invalid room type");
            }

        }

        public void TakeDamage(int damage)
        {
            Hp = (Hp - damage < 0) ? 0 : Hp - damage;
            if (IsDead)
            {
                StartCoroutine(DeadCoroutine(5f));
            }
        }

        public void TakeDamageToRoom(int damage, int roomId)
        {
            TakeDamage(damage);
            // 피격당한 room 객체 가져오기
            Room room = GetRoomByIdOrNull(roomId);
            if (room != null &&
                room.HasFacility)
            {
                room.FacilitySystem.TakeDamage(damage);
            }
        }

        public void RestoreHp(int damage)
        {
            if (IsDead) return;
            Hp = (Hp + damage > ShipData.MaxHp) ? ShipData.MaxHp : Hp + damage;
        }

        public void RequestAllocatePower(ERoomType facilityType)
        {
            if (IsDead) return;
            powerSystem.TryAllocatePower(facilityType);
        }

        public void RequestDeallocatePower(ERoomType facilityType)
        {
            if (IsDead) return;
            powerSystem.TryDeallocatePower(facilityType);
        }

        public virtual IEnumerator DeadCoroutine(float seconds)
        {
            yield break;
        }
    }
}
