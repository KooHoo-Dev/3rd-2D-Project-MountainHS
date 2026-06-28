using System;
using UnityEngine;

namespace FTL
{
    public class WeaponController : MonoBehaviour
    {
        private const float CHARGE_DURATION_ASSIST_MULTIPLIER = 0.75f;

        public bool IsReady => isReady;
        public bool IsPowered { set; get; } = false;
        public float ChargeProgress => chargeProgress;
        public float ChargeDuration => GetRealChargeDuration();

        [SerializeField] private float chargeProgress = 0f;

        [SerializeField] private float chargeDuration = 7f;

        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private bool isReady;
        [SerializeField] private bool prevIsPowered = false;
        [field: SerializeField] public bool IsAssisted { get; set; }
        public ProjectileTargetData TargetData;

        private void Awake()
        {
            isReady = false;
            IsAssisted = false;
            IsPowered = false;
            TargetData.TargetPosition = Vector3.zero;
            TargetData.RoomId = -1;
        }

        private void Update()
        {
            if (IsPowered)
            {
                if (prevIsPowered == false)
                {
                    chargeProgress = 0f;
                }

                chargeProgress = Mathf.Min(chargeProgress + Time.deltaTime, GetRealChargeDuration());
                // chargeSprites에 있는 스프라이트 수 - 1만큼 간격 검증
            }
            else
            {
                isReady = false;
                chargeProgress = 0f;
            }

            UpdateIsReady();

            // roomTarget이 유효하면 발사
            if (IsReadyToFire())
            {
                // 투사체에 타깃 정보 전달
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                projectileController.SetTarget(TargetData);
                projectile.transform.rotation = transform.rotation;
                TargetData.Reset();
                chargeProgress = 0f;
            }

            prevIsPowered = IsPowered;
        }

        private float GetRealChargeDuration()
        {
            return IsAssisted ? chargeDuration * CHARGE_DURATION_ASSIST_MULTIPLIER : chargeDuration;
        }

        private bool IsReadyToFire()
        {
            return TargetData.IsValid() && isReady;
        }

        private void UpdateIsReady()
        {
            if (chargeProgress == GetRealChargeDuration())
            {
                isReady = true;
            }
            else
            {
                isReady = false;
            }
        }

        public void SetRoomTarget(ProjectileTargetData targetData)
        {
            TargetData.TargetPosition = targetData.TargetPosition;
            TargetData.RoomId = targetData.RoomId;
        }

    }
}
