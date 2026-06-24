using UnityEngine;

namespace FTL
{
    public class ShieldSystem : FacilitySystem
    {
        public const int maxShieldLimit = 4;

        public int MaxShieldLimit => maxShieldLimit;
        public int MaxShield => maxShield;
        public int ShieldCount => shieldCount;
        public float ChargeDuration => chargeDuration;
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
            maxPower = 8;
            chargeProgress = 0f;
        }

        private void Update()
        {
            SetMaxShieldCountByPower(power);
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
                if (chargeProgress >= chargeDuration)
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
