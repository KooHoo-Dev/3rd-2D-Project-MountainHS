using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    public class WeaponSystem : FacilitySystem
    {
        [SerializeField] private List<Weapon> weapons;
        [SerializeField] private Weapon selectedWeapon;
        [SerializeField] private Vector2 selectedRoomTarget;

        protected override void Awake()
        {
            base.Awake();
            type = ERoomType.Weapon;
            selectedWeapon = null;
            selectedRoomTarget = Vector2.zero;
        }

        private void Update()
        {
            // 무기 시스템 전력 상태에 따라 무기 활성화 상태 변화
            if (weapons.Count > 0)
            {
                if (IsPowered)
                {
                    weapons[0].IsPowered = true;
                }
                else
                    weapons[0].IsPowered = false;
            }

            if (CanUpdateWeapon())
            {
                selectedWeapon.SetRoomTarget(selectedRoomTarget);
                selectedWeapon = null;
                selectedRoomTarget = Vector2.zero;
            }
        }

        private bool CanUpdateWeapon()
        {
            return selectedRoomTarget != Vector2.zero &&
                            selectedWeapon != null;
        }

        public void TickCharge(float deltaTime)
        {
            // TODO: 현재 전력에 따른 무기 충전 진행 규칙을 구체화한다.
        }

        public bool CanFire()
        {
            // TODO: 전력, 충전 상태, 조준 대상에 따른 발사 가능 여부를 구체화한다.
            return false;
        }

        public void Fire(Room targetRoom)
        {
            // TODO: 전투 시스템과 연결해 무기 발사 요청을 구체화한다.
        }

        public void SetSelectedWeapon(int index)
        {
            // 선택한 무기가 없으면 (무기 리스트의 인덱스 범위 초과) 예외처리
            if (index > weapons.Count)
            {
                Debug.LogError($"WeaponSystem: SetSelectWeapon index out of range");
                return;
            }

            // 선택한 무기 갱신
            selectedWeapon = weapons[index - 1];
        }

        public void SetSelectedRoomTarget(Vector2 target)
        {
            selectedRoomTarget = target;
        }

        // 선택한 무기 리셋
        public void ResetSelectedWeapon()
        {
            selectedWeapon = null;
        }
    }
}
