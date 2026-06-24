using FTL;
using System;
using UnityEngine;

namespace FTL
{
    public class Weapon : MonoBehaviour
    {
        public bool IsReady => isReady;
        public bool IsPowered { set; get; } = false;
        [field: SerializeField] public float ChargeProgress { get; private set; } = 0f;

        [field: SerializeField] public float ChargeDuration { get; private set; } = 7f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private bool isReady;
        [SerializeField] private bool prevIsPowered = false;
        [SerializeField] private Vector2 roomTarget;

        private void Awake()
        {
            isReady = false;
            IsPowered = false;
            roomTarget = Vector2.zero;
        }

        private void Update()
        {
            if (IsPowered)
            {
                if (prevIsPowered == false)
                    ChargeProgress = 0f;

                ChargeProgress = Mathf.Min(ChargeProgress + Time.deltaTime, ChargeDuration);

                // chargeSprites에 있는 스프라이트 수 - 1만큼 간격 검증
            }
            else
            {
                isReady = false;
                ChargeProgress = 0f;
            }

            UpdateIsReady();

            // roomTarget이 유효하면 발사
            if (IsReadyToFire())
            {
                // 투사체에 타깃 정보 전달
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                projectileController.SetTarget(roomTarget);
                projectile.transform.rotation = transform.rotation;
                ChargeProgress = 0f;
            }

            prevIsPowered = IsPowered;
        }

        private bool IsReadyToFire()
        {
            return roomTarget != Vector2.zero && isReady;
        }

        private void UpdateIsReady()
        {
            if (ChargeProgress == ChargeDuration)
            {
                isReady = true;
            }
            else
            {
                isReady = false;
            }
        }

        public void SetRoomTarget(Vector2 target)
        {
            roomTarget = target;
        }
    }
}
