using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace FTL
{
    public class ShopManager: SingletonBase<ShopManager>
    {
        private const int REACTOR_UPGRADE_MAX = 10;
        private const int FACILITY_UPGRADE_MAX = 10;
        // switch 두개 등록
        [SerializeField] private Button shipUpgradeSwitchButton;
        [SerializeField] private Button restoreSwitchButton;
        // 업그레이드 ui, 회복 ui
        [SerializeField] private GameObject upgradeUIRoot;
        [SerializeField] private GameObject restoreUIRoot;

        // 함선 업그레이드
        // 시설 최대 전력 한계 증가
        private List<int> facilityUpgradeCount = new List<int>();
        private List<int> facilityUpgradeMax = new List<int>();
        // 반응로 최대 전력 증가
        private int reactorUpgradeCount = 0;

        // 회복 ui
        // 승무원 회복
        // 함선 수리

        protected override void Awake()
        {
            base.Awake();
            InitFacilityUpgradeCounts();
            InitCrewHealViewers();
        }

        private void InitFacilityUpgradeCounts()
        {
            facilityUpgradeCount.Clear();
            facilityUpgradeMax.Clear();

            for (int i = 0; i < 3; i++)
            {
                facilityUpgradeCount.Add(0);
                facilityUpgradeMax.Add(FACILITY_UPGRADE_MAX);
            }
        }

        private void InitCrewHealViewers()
        {
            Transform[] children = restoreUIRoot.GetComponentsInChildren<Transform>(true);
            int crewIndex = 0;
            for (int i = 0; i < children.Length; i++)
            {
                //Transform child = children[i];
                //if (child.name != "Crew Summary")
                //{
                //    continue;
                //}

                //TMP_Text crewNameText = GetChildText(child, "Name");
                //Image hpBar = GetChildImage(child, "Hp Bar");
                //if (crewNameText == null || hpBar == null)
                //{
                //    continue;
                //}
                // 직접 crewStates 보고 crew summary를 만들어줘야함

                //CrewViewer crewViewer = child.GetComponent<CrewViewer>();
                //if (crewViewer == null)
                //{
                //    crewViewer = child.gameObject.AddComponent<CrewViewer>();
                //}

                //crewViewer.SetGameContextCrew(crewIndex, crewNameText, hpBar);
                //crewIndex++;
            }
        }

        private TMP_Text GetChildText(Transform root, string childName)
        {
            Transform child = root.Find(childName);

            return child.GetComponent<TMP_Text>();
        }

        private Image GetChildImage(Transform root, string childName)
        {
            Transform child = root.Find(childName);

            return child.GetComponent<Image>();
        }

        // 함선 업그레이드
        // 시설 최대 전력 한계 증가
        public void IncreaseMaxPowerLimitOnWeaponFacility(int amount = 1)
        {
            IncreaseMaxPowerLimit(ERoomType.Weapon, amount);
        }
        public void IncreaseMaxPowerLimtOnEngineFacility(int amount = 1)
        {
            IncreaseMaxPowerLimit(ERoomType.Engine, amount);
        }
        public void IncreaseMaxPowerLimitOnShieldFacility(int amount = 1)
        {
            IncreaseMaxPowerLimit(ERoomType.Shield, amount);
        }
        public void IncreaseMaxPowerLimit(ERoomType roomType, int amount=1)
        {
            if (amount <= 0)
            {
                return;
            }

            int currentScrap = GameManager.Instance.GetScrap();
            int price = 20 * amount;
            if (currentScrap < price)
            {
                Debug.LogWarning("Not enough scrap");
                return;
            }

            int index = (int)roomType;
            int upgradeCount = facilityUpgradeCount[index];
            int upgradeMax = facilityUpgradeMax[index];
            int newUpgradeCount = Mathf.Min(upgradeMax, upgradeCount + amount);
            int realIncrease = newUpgradeCount - upgradeCount;
            if (realIncrease == 0)
            {
                return;
            }

            facilityUpgradeCount[index] = newUpgradeCount;
            GameManager.Instance.IncreaseMaxPowerLimitOfFacility(roomType, realIncrease);
            GameManager.Instance.ReduceScrap(price);
        }

        // 반응로 최대 전력 증가
        public void IncreaseReactorMaxPower(int amount = 1)
        {
            if (amount <= 0)
            {
                return;
            }

            int currentScrap = GameManager.Instance.GetScrap();
            int price = 20 * amount;
            if (currentScrap < price)
            {
                Debug.LogWarning("Not enough scrap");
                return;
            }

            int newReactorUpgradeCount = Mathf.Min(REACTOR_UPGRADE_MAX, reactorUpgradeCount + amount);
            int realIncrease = newReactorUpgradeCount - reactorUpgradeCount;
            if (realIncrease == 0)
            {
                return;
            }

            reactorUpgradeCount = newReactorUpgradeCount;
            GameManager.Instance.IncreaseReactorMaxPower(realIncrease);
            GameManager.Instance.ReduceScrap(price);
        }

        // 회복 ui
        // 승무원 회복
        public void HealCrew(int crewIndex, int amount = 20)
        {
            if (amount <= 0)
            {
                return;
            }

            int currentScrap = GameManager.Instance.GetScrap();
            int price = 20 * (amount/20);
            if (currentScrap < price)
            {
                Debug.LogWarning("Not enough scrap");
                return;
            }

            int currentCrewHp = GameManager.Instance.GetCrewHp(crewIndex);
            int maxCrewHp = GameManager.Instance.GetCrewMaxHp(crewIndex);
            int newHp = Mathf.Min(maxCrewHp, currentCrewHp + amount);
            int realIncrease = newHp - currentCrewHp;

            if (realIncrease == 0)
                return;

            GameManager.Instance.AddCrewHp(crewIndex, realIncrease);
            GameManager.Instance.ReduceScrap(price);
        }

        // 함선 수리
        public void Repair(int amount = 1)
        {
            if (amount <= 0)
            {
                return;
            }

            int currentScrap = GameManager.Instance.GetScrap();
            int price = 20 * amount;
            if (currentScrap < price)
            {
                Debug.LogWarning("Not enough scrap");
                return;
            }

            int currentShipHp = GameManager.Instance.GetShipHp();
            int maxShipHp = GameManager.Instance.GetShipMaxHp();
            int newShipHp = Mathf.Min(maxShipHp, currentShipHp + amount);

            int realAmount = newShipHp - currentShipHp;
            if (realAmount == 0)
            {
                return;
            }

            GameManager.Instance.AddShipHp(amount);
            GameManager.Instance.ReduceScrap(price);
        }

        public void TurnShipUpgradeUIOn()
        {
            upgradeUIRoot.SetActive(true);
            restoreUIRoot.SetActive(false);
        }

        public void TurnRestoreUIOn()
        {
            upgradeUIRoot.SetActive(false);
            restoreUIRoot.SetActive(true);
        }
    }
}
