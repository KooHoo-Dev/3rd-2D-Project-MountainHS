using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace FTL
{
    [DefaultExecutionOrder(1000)]
    public class ShipController : MonoBehaviour, IDamageable
    {
        public bool IsDead => Hp == 0;
        [field: SerializeField] public ShipData ShipData { get; private set; }
        [field: SerializeField] public int Hp { get; private set; }
        [SerializeField] protected ShipPowerController powerSystem;
        protected List<FacilitySystem> facilitySystems = new List<FacilitySystem>() ;
        [field: SerializeField] public List<Room> Rooms { get; private set; } = new List<Room>();
        [field: SerializeField] public List<RoomViewer> RoomViewers { get; private set; } = new List<RoomViewer>();

        protected void Awake()
        {
            Hp = ShipData.MaxHp;
            facilitySystems.Add(GetComponent<WeaponSystem>());
            facilitySystems.Add(GetComponent<EngineSystem>());
            facilitySystems.Add(GetComponent<ShieldSystem>());

            for (int i = 0; i < ShipData.Rooms.Count; i++)
            {
                Room room = ShipData.Rooms[i];
                if (room.RoomType != ERoomType.Empty)
                    room.FacilitySystem = GetFacilitySystemByERoomType(room.RoomType);
                Rooms.Add(room);
            }

            RoomViewer[] roomViewers = GetComponentsInChildren<RoomViewer>();
            for (int i = 0; i < roomViewers.Length; i++)
            {
                RoomViewers.Add(roomViewers[i]);
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
                if (Rooms[i].RoomId == roomId)
                    return Rooms[i];
            }
            return null;
        }

        public FacilitySystem GetFacilitySystemByERoomType(ERoomType roomType)
        {
            switch (roomType)
            {
                case ERoomType.Weapon:
                    return facilitySystems[0];
                case ERoomType.Engine:
                    return facilitySystems[1];
                case ERoomType.Shield:
                    return facilitySystems[2];
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

        public IEnumerator DeadCoroutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            GameManager.Instance.HandleBattleResult(EBattleResult.EnemyShipDestroyed);
            Destroy(gameObject);
        }
    }
}
