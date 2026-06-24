using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    public class WeaponSystem : FacilitySystem
    {
        [SerializeField] private List<Weapon> weapons;
        [SerializeField] private Weapon selectedWeapon;

        protected override void Awake()
        {
            base.Awake();
            type = ERoomType.Weapon;
        }

        private void Update()
        {
            if (weapons.Count > 0)
            {
                if (IsPowered)
                {
                    weapons[0].IsPowered = true;
                }
                else
                    weapons[0].IsPowered = false;
            }
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

        public void SetSelectWeapon(int index)
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
    }
}
