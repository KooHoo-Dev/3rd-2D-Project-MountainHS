using System;
using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class FacilityPowerViewer : PowerViewer
    {
        [SerializeField] private Image facilityIcon;
        [SerializeField] private Image assistIcon;
        private Image repairBar;

        protected override void Awake()
        {
            base.Awake();
            facilityIcon = transform.GetChild(1).GetComponent<Image>();
            assistIcon = transform.GetChild(2).GetComponent<Image>();
            repairBar = transform.GetChild(3).GetComponent<Image>();
        }

        private void Start()
        {
            repairBar.enabled = false;
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
            UpdatePowerBars(power, maxPower, maxPowerLimit, totalDamage);
            UpdateRepairBar();
            UpdateFacilityIcon();
        }

        private void UpdateRepairBar()
        {
            if (repairBar.enabled == false)
                return;

            float repairDuration = facilitySystem.RepairDuration;
            float repairProgress = facilitySystem.RepairProgress;
            float newFillAmount = Mathf.InverseLerp(0f, repairDuration, repairProgress);
            repairBar.fillAmount = newFillAmount;
            if (newFillAmount == 1f)
            {
                repairBar.enabled = false;
            }
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

        private void UpdatePowerBars(int power, int maxPower, int maxPowerLimit, int totalDamage)
        {
            // 만약 현재 powerBarSprites 수가 실제 totalPower 와 일치하지 않는다면 생성
            if (barSprites.Count < maxPowerLimit)
            {
                int n = maxPowerLimit - barSprites.Count;
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
                    if (facilitySystem.RepairProgress > 0f && i == maxPowerLimit - totalDamage)
                    {
                        // 해당 PowerBar 위치로 repairBar 이동시키고 SetActive
                        repairBar.enabled = true;
                        RectTransform barTransform = barSprites[i].rectTransform;
                        repairBar.rectTransform.position = barTransform.position; 
                    }
                }
                else
                {
                    SetBarState(i, EPowerBarStatus.UnAvailable);
                }
            }
        }

        private void UpdateAssistImage()
        {
            assistIcon.enabled = facilitySystem.IsAssisted;
        }
    }
}
