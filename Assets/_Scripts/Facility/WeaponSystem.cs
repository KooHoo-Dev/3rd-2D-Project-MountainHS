using System;
using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    public class WeaponSystem : FacilitySystem
    {
        [SerializeField] private List<WeaponController> weapons;
        [SerializeField] private WeaponController selectedWeapon;
        [SerializeField] private ProjectileTargetData targetData;
        public ITargeting TargetingSystem { get; set; }

        protected override void Awake()
        {
            base.Awake();
            type = ERoomType.Weapon;
            selectedWeapon = null;
            targetData = new ProjectileTargetData(Vector3.zero, -1);
        }

        private void Start()
        {
            //WeaponController[] weapons = GetComponentsInChildren<WeaponController>();
            //for (int i = 0; i < weapons.Length; i++)
            //{
            //    this.weapons.Add(weapons[i]);
            //}
        }

        private void Update()
        {
            UpdateWeaponActivation();

            UpdateWeaponTargeting();
        }

        private void UpdateWeaponTargeting()
        {
            if (TargetingSystem == null)
            {
                if (CanUpdateWeapon())
                {
                    selectedWeapon.SetRoomTarget(targetData);
                    selectedWeapon = null;
                    targetData = new ProjectileTargetData(Vector3.zero, -1);
                }
            }
            else
            {
                // 적 무기 시스템은 플레이어 입력을 받지 않고
                // 랜덤하게 공격을 해야함
                // for 문으로 무기중 isPowered이면서 목표 방 정보가 없는 무기는
                // 조준 클래스의 방 정보 제공
                for (int i = 0; i < weapons.Count; i++)
                {
                    WeaponController weapon = weapons[i];
                    if (weapon.IsPowered &&
                        weapon.TargetData.IsValid() == false)
                    {
                        // 조준 클래스가 조준 정보 리턴
                        Room targetRoom = TargetingSystem.GetTarget();
                        targetData.RoomId = targetRoom.Id;
                        targetData.TargetPosition = targetRoom.GlobalPosition;

                        weapon.SetRoomTarget(targetData);
                    }
                }
            }
        }

        private void UpdateWeaponActivation()
        {
            // 무기 시스템 전력 상태에 따라 무기 활성화 상태 변화
            if (weapons.Count > 0)
            {
                if (IsPowered)
                {
                    for (int i = 0; i < weapons.Count; i++)
                    {
                        weapons[i].IsPowered = true;
                    }
                }
                else
                {
                    for (int i = 0; i < weapons.Count; i++)
                    {
                        weapons[i].IsPowered = false;
                    }
                }
            }
        }

        private bool CanUpdateWeapon()
        {
            return targetData.IsValid() &&
                            selectedWeapon != null;
        }

        public void SetSelectedWeapon(int index)
        {
            // 선택한 무기가 없으면 (무기 리스트의 인덱스 범위 초과) 예외처리
            if (index > weapons.Count)
            {
                Debug.LogWarning($"WeaponSystem: SetSelectWeapon index out of range");
                return;
            }

            // 선택한 무기 갱신
            selectedWeapon = weapons[index];
        }

        public void SetSelectedRoomTarget(ProjectileTargetData targetData)
        {
            this.targetData = targetData;
        }

        // 선택한 무기 리셋
        public void ResetSelectedWeapon()
        {
            selectedWeapon = null;
        }

        public override void ApplyAssistEffectOnFacility(bool isApplied)
        {
            base.ApplyAssistEffectOnFacility(isApplied);
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].ApplyAssistEffectOnWeapon(isApplied);
            }
        }

        public void AddWeapon(WeaponController weapon)
        {
            //weapons 초기화 해야함
            weapons.Add(weapon);
        }
    }
}
