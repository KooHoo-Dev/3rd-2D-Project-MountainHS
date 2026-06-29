using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utilities;

namespace FTL
{
    [DefaultExecutionOrder(1000)]
    public class ShipController : MonoBehaviour, IDamageable, IDeadable
    {
        private const float DEAD_ANIMATION_TIME = 5f;

        public bool IsDead => Hp == 0;
        public ShipOwnership Ownership { get; private set; }
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
        [SerializeField] private Transform crewSummaryGroupTransform;
        [SerializeField] private GameObject crewPrefab;
        [SerializeField] private GameObject crewSummaryPrefab;
        [SerializeField] protected int crewCount = 3;

        public List<CrewController> Crews { get; set; } = new List<CrewController>();
        protected List<FacilitySystem> facilitySystems = new List<FacilitySystem>() ;
        protected CrewSystem crewSystem;
        protected WeaponSystem weaponSystem;

        protected virtual void Awake()
        {
            Hp = ShipData.MaxHp;
            facilitySystems.Add(GetComponent<WeaponSystem>());
            facilitySystems.Add(GetComponent<EngineSystem>());
            facilitySystems.Add(GetComponent<ShieldSystem>());
            crewSystem = GetComponent<CrewSystem>();

            Ownership = GetComponentInChildren<ShipOwnership>();

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
                crewSystem.Rooms = Rooms;
            }

            void InitCrews()
            {
                for (int i = 0; i < crewCount; i++)
                {
                    GameObject crewObject = Instantiate(crewPrefab, crewGroupTransform);
                    CrewController crew = crewObject.GetComponent<CrewController>();
                    CrewViewer crewViewer = crew.GetComponent<CrewViewer>();
                    Room room = Rooms[i];
                    GameObject crewSummaryObject = Instantiate(crewSummaryPrefab, crewSummaryGroupTransform);
                    Image crewHpBar = crewSummaryObject?.GetComponentInChildren<Image>();
                    TMP_Text crewText = crewSummaryObject?.GetComponentInChildren<TMP_Text>();

                    if (crewHpBar != null)
                        crewViewer.SetHpBar(crewHpBar);
                    if (crewText != null)
                        crewViewer.SetText(crewText);
                    crew.InitStartRoom(room);
                    Crews.Add(crew);
                }
            }
        }

        protected void HandlePowerInput(ERoomType facilityType, bool isRecoverInput)
        {
            if (IsDead)
            {
                return;
            }

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
                bool isMatchingRoom = Rooms[i].Id == roomId;
                if (isMatchingRoom)
                {
                    return Rooms[i];
                }
            }

            return null;
        }

        protected Room GetRoomByPositionOrNull(Vector2 position)
        {
            for (int i = 0; i < Rooms.Count; i++)
            {
                bool isMatchingPosition = Rooms[i].GlobalPosition == position;
                if (isMatchingPosition)
                {
                    return Rooms[i];
                }
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
            int damagedHp = Hp - damage;
            Hp = damagedHp < 0 ? 0 : damagedHp;
        }

        public void TakeDamageToRoom(ProjectileController projectile, int roomId)
        {
            int damage = projectile.Damage;
            int crewDamage = projectile.CrewDamage;
            TakeDamage(damage);
            // 피격당한 room 객체 가져오기
            Room room = GetRoomByIdOrNull(roomId);
            room.TakeDamageToCrews(crewDamage);
            bool canDamageFacility = room != null && room.HasFacility;
            if (canDamageFacility)
            {
                room.TakeDamageToFacility(damage);
            }

            if (crewSystem.IsAllCrewDead)
            {
                EBattleResult battleResult = (Ownership.ShipSide == EShipSide.Player) 
                    ? EBattleResult.PlayerAllCrewDead 
                    : EBattleResult.EnemyAllCrewDead;
                StartCoroutine(DeadCoroutine(DEAD_ANIMATION_TIME, battleResult));
            }

            if (IsDead)
            {
                EBattleResult battleResult = (Ownership.ShipSide == EShipSide.Player) 
                    ? EBattleResult.PlayerShipDestroyed 
                    : EBattleResult.EnemyShipDestroyed;
                StartCoroutine(DeadCoroutine(DEAD_ANIMATION_TIME, battleResult));
            }
        }

        public void RestoreHp(int damage)
        {
            if (IsDead)
            {
                return;
            }

            int restoredHp = Hp + damage;
            Hp = restoredHp > ShipData.MaxHp ? ShipData.MaxHp : restoredHp;
        }

        public void RequestAllocatePower(ERoomType facilityType)
        {
            if (IsDead)
            {
                return;
            }

            powerSystem.TryAllocatePower(facilityType);
        }

        public void RequestDeallocatePower(ERoomType facilityType)
        {
            if (IsDead)
            {
                return;
            }

            powerSystem.TryDeallocatePower(facilityType);
        }

        public virtual IEnumerator DeadCoroutine(float seconds, EBattleResult result)
        {
            yield return new WaitForSeconds(seconds);
            GameManager.Instance.HandleBattleResult(result);
            gameObject.SetActive(false);
        }

        #region Test Method
        [ContextMenu("TakeDamage 1")]
        public void TestTakeDamage()
        {
            TakeDamage(1);
        }
        #endregion
    }
}
