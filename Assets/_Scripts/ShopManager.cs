using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace FTL
{
    public class ShopManager: MonoBehaviour
    {
        private const int REACTOR_UPGRADE_MAX = 10;
        private const int FACILITY_UPGRADE_MAX = 5;
        private const string SHIP_UPGRADE_SWITCH = "Ship Upgrade Switch";
        private const string RESTORE_SWITCH = "Restore Switch";
        private const string UPGRADE_UI_ROOT = "Upgrade UI Root";
        private const string RESTORE_UI_ROOT = "Restore UI Root";
        private const string CREW_GROUP_ROOT = "Crew Group Root";

        public static int BattlePrepareVisitCount = 0;

        // switch 두개 등록
        [SerializeField] private Button shipUpgradeSwitchButton;
        [SerializeField] private Button restoreSwitchButton;
        // 업그레이드 ui, 회복 ui
        [SerializeField] private Transform uiRoot;
        [SerializeField] private GameObject upgradeUIRoot;
        [SerializeField] private GameObject restoreUIRoot;
        [SerializeField] private GameObject crewSummaryPrefab;
        [SerializeField] private GameObject crewGroupRoot;

        // 함선 업그레이드
        // 시설 최대 전력 한계 증가
        private List<int> facilityUpgradeCount = new List<int>();
        private List<int> facilityUpgradeMax = new List<int>();
        // 반응로 최대 전력 증가
        private int reactorUpgradeCount = 0;

        // 회복 ui
        // 승무원 회복
        // 함선 수리

        private void Awake()
        {
            BattlePrepareVisitCount++;
            InitMainGameObject();
            InitFacilityUpgradeCounts();
            GameManager.Instance.EnsureBattlePrepareTestGameContext();
        }

        private void Start()
        {
            InitCrewSummaries();
        }

        private void InitMainGameObject()
        {
            upgradeUIRoot = GameObject.Find(UPGRADE_UI_ROOT);
            restoreUIRoot = GameObject.Find(RESTORE_UI_ROOT);
            upgradeUIRoot.SetActive(true);
            restoreUIRoot.SetActive(false);
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

        public void InitCrewSummaries()
        {
            int crewSummaryCount = GameManager.Instance.GetCrewCount();
            for (int i = 0; i < crewSummaryCount; i++)
            {
                GameObject crewSummary = Instantiate(crewSummaryPrefab, crewGroupRoot.transform);
                CrewViewer crewViewer = crewSummary.GetComponent<CrewViewer>();
                Button button = crewViewer.HealButton;
                int index = i;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => HealCrew(index));
                crewViewer.CrewIndex = i;
            }
        }

        // 함선 업그레이드
        // 시설 최대 전력 한계 증가
        public void IncreaseMaxPowerLimitOnWeaponFacility(int amount = 1)
        {
            IncreaseMaxPowerLimit(RoomType.Weapon, amount);
        }
        public void IncreaseMaxPowerLimtOnEngineFacility(int amount = 1)
        {
            IncreaseMaxPowerLimit(RoomType.Engine, amount);
        }
        public void IncreaseMaxPowerLimitOnShieldFacility(int amount = 1)
        {
            IncreaseMaxPowerLimit(RoomType.Shield, amount);
        }
        public void IncreaseMaxPowerLimit(RoomType roomType, int amount=1)
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

        public void ApplyGameContext(GameContext gameContext)
        {

            for (int i = 0; i < gameContext.FacilityUpgradeCount.Count; i++)
            {
                facilityUpgradeCount[i] = gameContext.FacilityUpgradeCount[i];
            }
            reactorUpgradeCount = gameContext.ReactorUpgradeCount;
        }
    }
}
