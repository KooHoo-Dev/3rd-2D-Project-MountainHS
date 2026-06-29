using System;
using UnityEngine;

namespace FTL
{
    public class ShieldSystem : FacilitySystem
    {
        private const int MAX_SHIELD_LIMIT = 4;
        private const float CHARGE_DURATION_ASSIST_MULTIPLIER = 0.75f;

        public int MaxShieldLimit => MAX_SHIELD_LIMIT;
        public int MaxShield => maxShield;
        public int ShieldCount => shieldCount;
        public float ChargeDuration => GetRealChargeDuration();
        public float ChargeProgress => chargeProgress;

        [Header("Shield")]
        [SerializeField] private int maxShield;
        [SerializeField] private int shieldCount;
        [SerializeField] private float chargeDuration = 3;
        [SerializeField] private float chargeProgress;

        protected override void Awake()
        {
            base.Awake();
            type = ERoomType.Shield;
            chargeProgress = 0f;
        }

        private void Update()
        {
            SetMaxShieldCountByPower(Power);
            UpdateShield();
        }

        /// <summary>
        /// 최대 실드 수까지 실드 채워지는 기능 구현
        /// 현재 실드 개수가 최대 실드 수보다 적으면 자동으로 증가 (실드 충전 시간 지나면)
        /// 현재 실드 수가 최대 실드 수에 도달하면 더이상 충전하지 않음
        /// 현재 실드 수 < 최대 실드 수 일때만 로직 실행
        /// </summary>
        private void UpdateShield()
        {
            if (shieldCount < maxShield)
            {
                chargeProgress += Time.deltaTime;
                if (chargeProgress >= GetRealChargeDuration())
                {
                    chargeProgress = 0f;
                    shieldCount++;
                }
            }
            else if (shieldCount > maxShield)
            {
                shieldCount = maxShield;
            }
            else
            {
                chargeProgress = 0f;
            }
        }

        private float GetRealChargeDuration()
        {
            return chargeDuration * (IsAssisted ? CHARGE_DURATION_ASSIST_MULTIPLIER : 1f);
        }

        /// <summary>
        /// 전력 개수에 따라 최대 실드 수 결정
        /// </summary>
        /// <param name="power"></param>
        private void SetMaxShieldCountByPower(int power)
        {
            maxShield = power / 2;
        }

        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
        }

        public override void ApplyAssistEffectOnFacility(bool isApplied)
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

        public void TakeDamageToShield()
        {
            if (shieldCount == 0)
            {
                Debug.Log("ShieldCount is zero, damage to Hull");
                return;
            }

            shieldCount--;
        }


    }
}
