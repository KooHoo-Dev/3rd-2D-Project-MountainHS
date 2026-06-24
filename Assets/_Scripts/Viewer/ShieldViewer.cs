using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class ShieldViewer : MonoBehaviour
    {
        [SerializeField] private Image[] shieldSquares;
        [SerializeField] private Image shieldChargeBar;
        [SerializeField] private SpriteRenderer shieldMainSprite;
        [SerializeField] private ShieldSystem shieldSystem;
        [SerializeField] private int prevShieldCount;
        [SerializeField] private Color color;

        // 실드 시스템의 최대 실드 개수, 실드 개수에 따라 실드 ui 표시
        private void LateUpdate()
        {
            UpdateViewer();
        }

        // 충전한 정도에 따라서 투명도를 다르게 표시하고 싶은데
        private void UpdateViewer()
        {
            int maxShield = shieldSystem.MaxShield;
            int shieldCount = shieldSystem.ShieldCount;
            int maxShieldLimit = shieldSystem.MaxShieldLimit;

            //if (shieldCount == maxShield
            //    && prevShieldCount == shieldCount)
            //    return;

            UpdateShieldSquares(maxShield, shieldCount, maxShieldLimit);

            UpdateMainShield(shieldCount, maxShieldLimit);

            UpdateChargeBar();

            void UpdateChargeBar()
            {
                float chargeProgress = shieldSystem.ChargeProgress;
                float chargeDuration = shieldSystem.ChargeDuration;
                float chargeRate = chargeProgress / chargeDuration;
                shieldChargeBar.fillAmount = chargeRate;
            }

            void UpdateShieldSquares(int maxShield, int shieldCount, int maxShieldLimit)
            {
                for (int i = 0; i < maxShieldLimit; i++)
                {
                    Color newUIColor = color;
                    if (i < shieldCount)
                        newUIColor.a = 1f;
                    else if (i < maxShield)
                        newUIColor.a = 0.5f;
                    else
                        newUIColor.a = 0f;

                    shieldSquares[i].color = newUIColor;
                }
            }

            void UpdateMainShield(int shieldCount, int maxShieldLimit)
            {
                Color newMainColor = Color.white;
                newMainColor.a = (float)shieldCount / maxShieldLimit;
                shieldMainSprite.color = newMainColor;
            }
        }
    }
}
