using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class WeaponViewer : MonoBehaviour
    {
        [SerializeField] private Sprite[] chargeSprites;
        [SerializeField] private Weapon weapon;
        [SerializeField] private Image weaponChargeBar;
        private SpriteRenderer spriteRenderer;

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
        private void UpdateChargeSprite(float chargeRate)
        {
            int spriteIndex = Mathf.FloorToInt((chargeRate) * (chargeSprites.Length - 1));
            spriteRenderer.sprite = chargeSprites[spriteIndex];
        }
    }
}
