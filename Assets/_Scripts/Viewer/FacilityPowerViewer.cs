using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class FacilityPowerViewer : PowerViewer
    {
        [SerializeField] private Image facilityIcon;
        [SerializeField] private Image assistImage;
        private FacilitySystem facilitySystem;

        protected override void Awake()
        {
            base.Awake();
            facilitySystem = powerSystemMono as FacilitySystem;
        }

        protected override void SetBarState(int barIndex, EPowerBarStatus status)
        {
            base.SetBarState(barIndex, status);
        }

        protected override void UpdateViewer()
        {
            int maxPower = facilitySystem.MaxPower;
            int power = facilitySystem.Power;
            int maxPowerLimit = facilitySystem.MaxPowerLimit;
            int totalDamage = facilitySystem.TotalDamage;

            UpdateAssistImage();
            UpdatePowerBars(maxPower, power, maxPowerLimit, totalDamage);
            UpdateFacilityIcon();
        }

        private void UpdateFacilityIcon()
        {
            // 만약 할당된 전력이 0이라면 시설 아이콘 색깔 비활성화
            switch (facilitySystem.FacilityStatus)
            {
                case EFacilityStatus.Normal:
                    facilityIcon.color = Color.green;
                    break;
                case EFacilityStatus.Damaged:
                    facilityIcon.color = Color.orange;
                    break;
                case EFacilityStatus.Broked:
                    facilityIcon.color = Color.red;
                    break;
                case EFacilityStatus.Disabled:
                    facilityIcon.color = Color.grey;
                    break;
                default:
                    Debug.LogWarning("Unknown Facility Status");
                    break;
            }
        }

        private void UpdatePowerBars(int maxPower, int power, int maxPowerLimit, int totalDamage)
        {
            // 만약 현재 powerBarSprites 수가 실제 totalPower 와 일치하지 않는다면 생성
            if (barSprites.Count < maxPower)
            {
                int n = maxPower - barSprites.Count;
                for (int i = 0; i < n; i++)
                {
                    AddBar();
                }
            }
            for (int i = 0; i < barSprites.Count; i++)
            {
                if (i < power)
                {
                    SetBarState(i, EPowerBarStatus.Available);
                }
                else if (totalDamage > 0 && i >= maxPowerLimit - totalDamage)
                {
                    SetBarState(i, EPowerBarStatus.Broken);
                }
                else
                {
                    SetBarState(i, EPowerBarStatus.UnAvailable);
                }
            }
        }

        private void UpdateAssistImage()
        {
            assistImage.enabled = facilitySystem.IsAssisted;
        }
    }
}
