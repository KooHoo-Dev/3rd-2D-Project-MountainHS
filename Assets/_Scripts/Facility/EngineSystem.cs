using UnityEngine;

namespace FTL
{
    public class EngineSystem : FacilitySystem
    {
        public float EscapeChargeProgress => escapeChargeProgress;
        public float EscapeChargeTime => escapeChargeDuration;
        public bool CanEscape => canEscape;

        [SerializeField] private float escapeChargeProgress;
        [SerializeField] private float escapeChargeDuration = 90f;
        [SerializeField] private bool canEscape;
        [SerializeField] private int prevPower;
        [SerializeField] private float chargeSpeed;

        protected override void Awake()
        {
            base.Awake();
            type = ERoomType.Engine;
        }

        private void Update()
        {
            UpdateEscapeCharge();
            UpdateCanEscape();
            prevPower = power;
        }

        private void UpdateCanEscape()
        {
            if (escapeChargeProgress == escapeChargeDuration)
            {
                canEscape = true;
            }
            else
            {
                canEscape = false;
            }
        }

        public void UpdateEscapeCharge()
        {
            // TODO: 현재 전력에 따른 도망 준비 충전 규칙을 구체화한다.
            if (power == 0)
            {
                escapeChargeProgress = 0f;
                return;
            }

            chargeSpeed = GetNewChargeSpeed(ref chargeSpeed);

            escapeChargeProgress += Time.deltaTime * chargeSpeed;
            if (escapeChargeProgress >= escapeChargeDuration)
            {
                escapeChargeProgress = escapeChargeDuration;
            }
        }

        private float GetNewChargeSpeed(ref float chargeSpeed)
        {
            if (prevPower == power)
            {
                return chargeSpeed;
            }

            if (power == 0)
            {
                return 0f;
            }

            return 1 + 0.5f * (power - 1);
        }
    }
}
