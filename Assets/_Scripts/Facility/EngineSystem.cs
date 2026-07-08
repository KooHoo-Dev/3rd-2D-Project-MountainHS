using UnityEngine;

namespace FTL
{
    public class EngineSystem : FacilitySystem
    {
        private const float CHARGE_SPEED_ASSIST_MULTIPLIER = 1.2f;

        public float ChargeProgress => chargeProgress;
        public float ChargeDuration => chargeDuration;
        public bool CanEscape => canEscape;

        [SerializeField] private float chargeProgress;
        [SerializeField] private float chargeDuration = 90f;
        [SerializeField] private bool canEscape;
        [SerializeField] private int prevPower;
        [SerializeField] private float chargeSpeed = 1f;

        #region Unity Method

        protected override void Awake()
        {
            base.Awake();
            type = RoomType.Engine;
        }

        private void Update()
        {
            if (CanRunFacilityLogic() == false)
            {
                return;
            }

            UpdateEscapeCharge();
            UpdateCanEscape();
            prevPower = Power;
        }

        #endregion

        #region Public Method

        public void UpdateEscapeCharge()
        {
            if (Power == 0)
            {
                chargeProgress = 0f;
                return;
            }

            chargeSpeed = GetNewChargeSpeed(chargeSpeed);

            chargeProgress += Time.deltaTime * chargeSpeed;
            chargeProgress = Mathf.Min(chargeProgress, chargeDuration);
        }

        #endregion

        #region Private Method

        private void UpdateCanEscape()
        {
            if (ChargeProgress == chargeDuration)
            {
                canEscape = true;
            }
            else
            {
                canEscape = false;
            }
        }

        private float GetNewChargeSpeed(float chargeSpeed)
        {
            if (Power == 0)
            {
                return 0f;
            }

            return (1 + 0.5f * (Power - 1)) 
                * (IsAssisted ? CHARGE_SPEED_ASSIST_MULTIPLIER : 1f);
        }

        #endregion
    }
}
