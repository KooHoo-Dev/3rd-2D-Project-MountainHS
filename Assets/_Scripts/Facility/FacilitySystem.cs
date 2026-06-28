using Unity.VisualScripting;
using UnityEngine;

namespace FTL
{
    public class FacilitySystem : MonoBehaviour, IPowerInfo
    {
        public ERoomType Type => type;
        public EFacilityStatus FacilityStatus => facilityStatus;
        //public int MaxHp => maxHp;
        //public int Hp => hp;
        public int MaxPowerLimit => maxPowerLimit;
        public int MaxPower => maxPower;
        public int Power => power;
        public int TotalDamage => totalDamage;
        public bool IsPowered => power > 0;
        [field: SerializeField] public float RepairDuration { get; set; } = 7.5f;
        [field: SerializeField] public float RepairProgress { get; set; } = 0f;
        [SerializeField] protected ERoomType type;
        [SerializeField] protected int maxPowerLimit = 3;
        [SerializeField] protected int maxPower;
        [SerializeField] protected int power;
        [SerializeField] protected int totalDamage;
        [SerializeField] protected bool isAssisted = false;
        protected EFacilityStatus facilityStatus;

        protected virtual void Awake()
        {
            //hp = maxHp;
            power = 0;
            maxPower = maxPowerLimit;
            facilityStatus = EFacilityStatus.Normal;
            totalDamage = 0;
            UpdateFacilityStatus();
        }

        public bool CanAddPower(int amount)
        {
            // TODO: 시설 상태, 체력, 최대 전력에 따른 전력 설정 가능 여부를 구체화한다.
            //bool isPowerLowerThanMaxPower = power < maxPower;
            //bool isMaxPowerLowerThanMaxPowerLimit = maxPower < maxPowerLimit;
            if (amount <= 0)
            {
                return false;
            }

            return power + amount <= maxPower;
        }

        public void AddPower(int amount)
        {
            // TODO: 전력 변경 검증 및 변경 후 시설별 효과 갱신 호출을 구체화한다.
            if (CanAddPower(amount))
            {
                power += amount;
            }
            UpdateFacilityStatus();
        }

        public void ReducePower(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            power = Mathf.Max(power - amount, 0);
            UpdateFacilityStatus();
        }

        public virtual void TakeDamage(int amount)
        {
            int newDamage = totalDamage + amount;
            newDamage = Mathf.Min(newDamage, maxPowerLimit);
            totalDamage = newDamage;
            maxPower = maxPowerLimit - newDamage;

            UpdateFacilityStatus();
        }

        public void Repair(int amount)
        {
            totalDamage = Mathf.Max(totalDamage-amount, 0);

            maxPower = maxPowerLimit - totalDamage;
            UpdateFacilityStatus();
        }

        public void UpdateFacilityStatus()
        {
            if (totalDamage == maxPowerLimit)
            {
                facilityStatus = EFacilityStatus.Broked;
            }
            else if (totalDamage > 0 && totalDamage < maxPowerLimit)
            {
                facilityStatus = EFacilityStatus.Damaged;
            }
            else if (power == 0)
            {
                facilityStatus = EFacilityStatus.Disabled;
            }
            else if (totalDamage == 0)
            {
                facilityStatus = EFacilityStatus.Normal;
            }
        }

        public int HandlePowerOverflow()
        {
            int overflowAmount = power - maxPower;
            if (overflowAmount < 0)
            {
                return 0;
            }

            return overflowAmount;
        }

        public virtual void ApplyCrewAssistEffect(bool isApplied)
        {
            if (isApplied != isAssisted)
                Debug.Log($"Apply crew assist! in {this}! before: {isAssisted} after: {isApplied}");
            isAssisted = isApplied;
        }

        #region Context Menu Test Function
        [ContextMenu("Add Power")]
        public void TestAddPower()
        {
            // TODO: 전력 변경 검증 및 변경 후 시설별 효과 갱신 호출을 구체화한다.
            AddPower(1);
        }

        [ContextMenu("Reduce Power")]
        public void TestReducePower()
        {
            ReducePower(1);
        }

        [ContextMenu("Take Damage")]
        public void TestTakeDamage()
        {
            TakeDamage(1);
        }

        [ContextMenu("Repair")]
        public void TestRepair()
        {
            Repair(1);
        }
        #endregion
    }
}
