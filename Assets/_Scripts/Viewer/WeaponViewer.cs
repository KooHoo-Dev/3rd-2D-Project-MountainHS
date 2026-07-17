using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class WeaponViewer : MonoBehaviour
    {
        [SerializeField] private Sprite[] chargeSprites;
        [SerializeField] private WeaponController weapon;
        [SerializeField] private Image weaponChargeBar;
        private SpriteRenderer spriteRenderer;

        #region Public Method

        public void SetChargeBar(Image chargeBar)
        {
            weaponChargeBar = chargeBar;
        }

        #endregion

        #region Private Method

        private void UpdateChargeSprite(float chargeRate)
        {
            int spriteIndex = Mathf.FloorToInt(chargeRate * (chargeSprites.Length - 1));
            spriteRenderer.sprite = chargeSprites[spriteIndex];
        }

        #endregion

        #region Unity Method

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            float chargeRate = weapon.ChargeProgress / weapon.ChargeDuration;
            UpdateChargeSprite(chargeRate);
            weaponChargeBar.fillAmount = chargeRate;
        }

        #endregion
    }
}
