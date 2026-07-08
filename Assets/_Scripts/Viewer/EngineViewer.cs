using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class EngineViewer: MonoBehaviour
    {
        [SerializeField] private GameObject chargeStatus;
        [SerializeField] private GameObject jumpButton;
        [SerializeField] private EngineSystem engineSystem;
        [SerializeField] private Image jumpProgressBar;
        [SerializeField] private Image assistImage;

        #region Unity Method

        private void Awake()
        {
            jumpButton.SetActive(false);
            chargeStatus.SetActive(true);
        }

        private void LateUpdate()
        {
            bool canJump = UpdateChargeBar();

            UpdateJumpButton(canJump);

            bool UpdateChargeBar()
            {
                float escapeChargeProgress = engineSystem.ChargeProgress;
                float escapeChargeDuration = engineSystem.ChargeDuration;
                float escapeChargeRate = escapeChargeProgress / escapeChargeDuration;
                bool canJump = engineSystem.CanEscape;

                jumpProgressBar.fillAmount = escapeChargeRate;
                return canJump;
            }

            void UpdateJumpButton(bool canJump)
            {
                if (canJump)
                {
                    chargeStatus.SetActive(false);
                    jumpButton.SetActive(true);
                }
                else
                {
                    chargeStatus.SetActive(true);
                    jumpButton.SetActive(false);
                }
            }

        }

        #endregion
    }
}
