using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FTL
{
    public class WeaponController : MonoBehaviour
    {
        private const float CHARGE_DURATION_ASSIST_MULTIPLIER = 0.75f;

        public bool IsReady => isReady;
        public bool IsPowered { set; get; } = false;
        public float ChargeProgress => chargeProgress;
        public float ChargeDuration => GetRealChargeDuration();

        [field: SerializeField] public bool IsAssisted { get; set; }
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float chargeProgress = 0f;

        [SerializeField] private float chargeDuration = 7f;
        [SerializeField] private float fireDelay;
        [SerializeField] private bool isReady;
        [SerializeField] private bool prevIsPowered = false;
        [SerializeField] private int fireCount;
        public ProjectileTargetData TargetData;
        private WeaponSystem weaponSystem;
        private ITargeting targetingSystem = null;

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
                // 플레이어 발사 로직
                float seed = Random.Range(0f, 4f);
                seed = seed == 4f ? 0f : seed;
                if (targetingSystem == null)
                {
                    StartCoroutine(FireCoroutine(TargetData, seed));
                }
                TargetData.Reset();
                chargeProgress = 0f;
                weaponSystem.ResetWeaponTargetSprite(this);
            }
            else if (IsReady && targetingSystem != null)
            {
                // 자체 조준 로직을 가지고 있는 적 발사 로직
                float seed = Random.Range(0f, 4f);
                seed = seed == 4f ? 0f : seed;
                StartCoroutine(FireCoroutine(seed));
                chargeProgress = 0f;
            }

            prevIsPowered = IsPowered;
        }

        private IEnumerator FireCoroutine(ProjectileTargetData targetData, float seed)
        {
            for (int i = 0; i < fireCount; i++)
            {
                // 투사체에 타깃 정보 전달
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                projectileController.SetTarget(targetData);
                projectileController.SetRandomSeed(seed);
                projectile.transform.rotation = transform.rotation;
                yield return new WaitForSeconds(fireDelay);
            }
        }

        private IEnumerator FireCoroutine(float seed)
        {
            for (int i = 0; i < fireCount; i++)
            {
                // 투사체에 타깃 정보 전달
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                ProjectileController projectileController = projectile.GetComponent<ProjectileController>();
                ProjectileTargetData targetData = new ProjectileTargetData();
                Room targetRoom = targetingSystem.GetTarget();
                targetData.RoomId = targetRoom.Id;
                targetData.TargetPosition = targetRoom.GlobalPosition;
                projectileController.SetTarget(targetData);
                projectileController.SetRandomSeed(seed);
                projectile.transform.rotation = transform.rotation;
                yield return new WaitForSeconds(fireDelay);
            }
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

        public void SetTargetingSystem(ITargeting targetingSystem)
        {
            this.targetingSystem = targetingSystem;
        }

        public void SetWeaponSystem(WeaponSystem weaponSystem)
        {
            this.weaponSystem = weaponSystem;
        }

        public void ApplyAssistEffectOnWeapon(bool isApplied)
        {
            if (IsAssisted != isApplied)
            {
                if (isApplied == true)
                {
                    float currentRate = chargeProgress / GetRealChargeDuration();
                    chargeProgress = Mathf.Lerp(0f, chargeDuration, currentRate);
                }
                IsAssisted = isApplied;
                chargeProgress = Mathf.Min(chargeProgress, GetRealChargeDuration());
            }
        }

    }
}
