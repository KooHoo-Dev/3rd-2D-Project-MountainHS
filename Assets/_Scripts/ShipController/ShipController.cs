    using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    [DefaultExecutionOrder(1000)]
    public class ShipController : MonoBehaviour, IDamageable, IDeadable
    {
        private const float DEAD_ANIMATION_TIME = 5f;

        public bool IsDead => Hp == 0;
        public ShipOwnership Ownership { get; private set; }
        [Header("Start Weapon Types")]
        [field: SerializeField] public List<EWeaponType> WeaponTypes { get; private set; } = new List<EWeaponType>();
        [SerializeField] protected GameObject weaponChargeStatusPrefab;
        [SerializeField] protected List<WeaponTransformData> weaponTransformList;

        [Header("Ship Data")]
        [field: SerializeField] public ShipData ShipData { get; private set; }

        [field: SerializeField] public int Hp { get; set; }

        [Header("Power System")]
        public ShipPowerController PowerSystem;

        [Header("Room")]
        [SerializeField] private GameObject roomPrefab;
        [SerializeField] private Transform roomGroupTransform;
        [field: SerializeField] public List<Room> Rooms { get; private set; } = new List<Room>();

        [Header("Crew")]
        [SerializeField] private Transform crewGroupTransform;
        [SerializeField] private Transform crewSummaryGroupTransform;
        [SerializeField] protected Transform weaponGroupTransform;
        [SerializeField] protected Transform weaponChargeGroupTransform;
        [SerializeField] private GameObject crewPrefab;
        [SerializeField] private GameObject crewSummaryPrefab;
        [SerializeField] protected int crewCount = 3;
        [SerializeField] protected bool isPlayer = false;

        public List<CrewController> Crews { get; set; } = new List<CrewController>();
        public List<FacilitySystem> FacilitySystems = new List<FacilitySystem>();
        protected CrewSystem crewSystem;
        protected WeaponSystem weaponSystem;
        protected bool isDeadCoroutineActivated;
        protected bool isShipInitialized;

        protected virtual void Awake()
        {
            InitShipReferences();
            TryInitializeShip(false);
        }

        public void SetExternalUIGroups(Transform crewSummaryGroup, Transform weaponChargeGroup)
        {
            crewSummaryGroupTransform = crewSummaryGroup;
            weaponChargeGroupTransform = weaponChargeGroup;
        }

        public void InitializeShipIfReady()
        {
            TryInitializeShip(true);
        }

        private void InitShipReferences()
        {
            if (ShipData != null)
            {
                Hp = ShipData.MaxHp;
            }

            FacilitySystems.Clear();
            FacilitySystems.Add(GetComponent<WeaponSystem>());
            FacilitySystems.Add(GetComponent<EngineSystem>());
            FacilitySystems.Add(GetComponent<ShieldSystem>());
            crewSystem = GetComponent<CrewSystem>();

            Ownership = GetComponentInChildren<ShipOwnership>();

            weaponSystem = (WeaponSystem)GetFacilitySystemByERoomType(ERoomType.Weapon);
            PowerSystem = GetComponent<ShipPowerController>();
            isDeadCoroutineActivated = false;
            isShipInitialized = false;
        }

        private void TryInitializeShip(bool shouldLogError)
        {
            if (isShipInitialized)
            {
                return;
            }

            if (CanInitializeShip(shouldLogError) == false)
            {
                return;
            }

            InitRooms();
            InitCrews();
            InitWeapons();
            isShipInitialized = true;
        }

        private bool CanInitializeShip(bool shouldLogError)
        {
            bool canInitialize =
                ShipData != null &&
                roomPrefab != null &&
                roomGroupTransform != null &&
                crewGroupTransform != null &&
                crewSummaryGroupTransform != null &&
                weaponGroupTransform != null &&
                weaponChargeGroupTransform != null &&
                crewPrefab != null &&
                crewSummaryPrefab != null &&
                weaponChargeStatusPrefab != null &&
                crewSystem != null &&
                weaponSystem != null &&
                PowerSystem != null;

            if (canInitialize == false && shouldLogError)
            {
                Debug.LogError($"{name}: Ship initialization failed. Required prefab or group reference is missing.");
            }

            return canInitialize;
        }

        private void InitRooms()
            {
                for (int i = 0; i < ShipData.Rooms.Count; i++)
                {
                    GameObject roomObject = Instantiate(roomPrefab, roomGroupTransform);
                    RoomViewer roomViewer = roomObject.GetComponent<RoomViewer>();
                    Room room = new Room(ShipData.Rooms[i]);
                    // RoomData 초기화 함수 생성
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

        private void InitCrews()
            {
                for (int i = 0; i < crewCount; i++)
                {
                    GameObject crewObject = Instantiate(crewPrefab, crewGroupTransform);
                    CrewController crew = crewObject.GetComponent<CrewController>();
                    Room room = Rooms[i];
                    GameObject crewSummaryObject = Instantiate(crewSummaryPrefab, crewSummaryGroupTransform);
                    CrewViewer crewViewer = crewSummaryObject.GetComponent<CrewViewer>();
                    SpriteRenderer crewSprite = crewObject.transform.Find("Crew Sprite").GetComponent<SpriteRenderer>();
                    SpriteRenderer crewTargetSprite = crewObject.transform.Find("Target Room").GetComponent<SpriteRenderer>();

                    crewViewer.SetCrewSprite(crewSprite);
                    crewViewer.SetCrewTargetSpite(crewTargetSprite);
                    crewViewer.SetCrewController(crew);
                    crew.InitStartRoom(room);
                    Crews.Add(crew);
                }
            }

        private void InitWeapons()
            {
                for (int i = 0; i < WeaponTypes.Count; i++)
                {
                    EWeaponType weaponType = WeaponTypes[i];
                    WeaponDatabase weaponDatabase = GameManager.Instance.WeaponDatabase;
                    GameObject weaponPrefab = weaponDatabase.GetPrefab(weaponType);
                    GameObject weaponObject = Instantiate(weaponPrefab, weaponGroupTransform);
                    GameObject weaponChargeObject = Instantiate(weaponChargeStatusPrefab, weaponChargeGroupTransform);
                    WeaponViewer weaponViewer = weaponObject.GetComponent<WeaponViewer>();
                    WeaponController weapon = weaponObject.GetComponent<WeaponController>();
                    Image chargeImage = weaponChargeObject.transform.GetChild(1)
                        .GetChild(1)
                        .GetComponent<Image>();
                    Button weaponButton = weaponChargeObject.GetComponentInChildren<Button>();
                    SpriteRenderer weaponSprite = weaponObject.GetComponentInChildren<SpriteRenderer>();
                    Vector3 weaponOffset = weaponTransformList[i].WeaponOffset;
                    Vector3 weaponRotation = weaponTransformList[i].WeaponRotation;
                    bool isFlipX = weaponTransformList[i].isFlipX;
                    bool isFlipY = weaponTransformList[i].isFlipY;
                    int index = i;

                    if (isPlayer)
                    {
                        weaponButton.onClick.RemoveAllListeners();
                        weaponButton.onClick.AddListener(() => weaponSystem.SetSelectedWeapon(index));
                    }
                    weaponObject.transform.localPosition += weaponOffset;
                    weaponObject.transform.Rotate(weaponRotation);
                    weaponSprite.flipX = isFlipX;
                    weaponSprite.flipY = isFlipY;
                    weaponViewer.SetChargeBar(chargeImage);
                    weaponSystem.AddWeapon(weapon);
                }
                weaponSystem.SetWeaponSystemOfWeapons();
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
                    return FacilitySystems[(int)ERoomType.Weapon];
                case ERoomType.Engine:
                    return FacilitySystems[(int)ERoomType.Engine];
                case ERoomType.Shield:
                    return FacilitySystems[(int)ERoomType.Shield];
                default:
                    throw new ArgumentException("Invalid room type");
            }

        }

        public void SetWeaponTypes(List<EWeaponType> weaponTypes)
        {
            WeaponTypes = new List<EWeaponType>();

            for (int i = 0; i < weaponTypes.Count; i++)
            {
                WeaponTypes.Add(weaponTypes[i]);
            }
        }

        public void TakeDamage(int damage)
        {
            Hp = Mathf.Max(0, Hp - damage);
        }

        public void TakeDamageToRoom(ProjectileController projectile, int roomId)
        {
            int damage = projectile.Damage;
            int crewDamage = projectile.CrewDamage;
            TakeDamageToRoom(damage, crewDamage, roomId);
        }

        public void TakeDamageToRoom(int damage, int crewDamage, int roomId)
        {
            // 피격당한 room 객체 가져오기
            Room room = GetRoomByIdOrNull(roomId);
            if (room == null)
            {
                Debug.LogError($"{name}: Cannot apply room damage. Room id {roomId} was not found.");
                return;
            }

            room.TakeDamageToCrews(crewDamage);
            StartCoroutine(ShakeRoom(room));
            bool canDamageFacility = room != null && room.HasFacility;
            if (canDamageFacility)
            {
                room.TakeDamageToFacility(damage);
            }
            TakeDamage(damage);

            CheckGameOver();
        }

        private IEnumerator ShakeRoom(Room room)
        {
            room.SetIsHit(true);
            yield return new WaitForSeconds(0.25f);
            room.SetIsHit(false);
        }

        private void CheckGameOver()
        {
            EBattleResult battleResult = EBattleResult.None;
            bool isGameOver = IsDead || crewSystem.IsAllCrewDead;
            if (isGameOver == false || isDeadCoroutineActivated == true)
            {
                return;
            }

            if (crewSystem.IsAllCrewDead)
            {
                battleResult = (Ownership.ShipSide == EShipSide.Player)
                    ? EBattleResult.PlayerAllCrewDead
                    : EBattleResult.EnemyAllCrewDead;
            }
            if (IsDead)
            {
                battleResult = (Ownership.ShipSide == EShipSide.Player)
                    ? EBattleResult.PlayerShipDestroyed
                    : EBattleResult.EnemyShipDestroyed;
            }

            StartCoroutine(DeadCoroutine(DEAD_ANIMATION_TIME, battleResult));
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

            PowerSystem.TryAllocatePower(facilityType);
        }

        public void RequestDeallocatePower(ERoomType facilityType)
        {
            if (IsDead)
            {
                return;
            }

            PowerSystem.TryDeallocatePower(facilityType);
        }

        public virtual IEnumerator DeadCoroutine(float seconds, EBattleResult result)
        {
            isDeadCoroutineActivated = true;
            yield return new WaitForSeconds(seconds);
            GameManager.Instance.HandleBattleResult(result);
            gameObject.SetActive(false);
        }

        #region Test Method
        [ContextMenu("TakeDamage 1")]
        public void TestTakeDamage()
        {
            TakeDamage(1);
            CheckGameOver();
        }
        #endregion
    }
}
