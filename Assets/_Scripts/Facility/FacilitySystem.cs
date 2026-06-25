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

        [SerializeField] protected ERoomType type;
        //[SerializeField] protected int maxHp;
        //[SerializeField] protected int hp;
        [SerializeField] protected int maxPowerLimit = 3;
        [SerializeField] protected int maxPower;
        [SerializeField] protected int power;
        [SerializeField] protected int totalDamage;
        protected EFacilityStatus facilityStatus;

        protected virtual void Awake()
        {
            //hp = maxHp;
            power = 0;
            maxPower = maxPowerLimit;
            facilityStatus = EFacilityStatus.Normal;
            totalDamage = 0;
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
        }

        public void ReducePower(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            power = Mathf.Max(power - amount, 0);
        }

        public virtual void TakeDamage(int amount)
        {
            // TODO: 시설 피해 처리와 상태 변경 규칙을 구체화한다.
            int newDamage = totalDamage + amount;
            newDamage = Mathf.Min(newDamage, maxPowerLimit);
            totalDamage = newDamage;
            maxPower = maxPowerLimit - newDamage;

            UpdateFacilityStatus();
        }

        public void UpdateFacilityStatus()
        {
            if (totalDamage == 0)
            {
                facilityStatus = EFacilityStatus.Normal;
            }
            else if (totalDamage < maxPowerLimit)
            {
                facilityStatus = EFacilityStatus.Damaged;
            }
            else if (totalDamage == maxPowerLimit)
            {
                facilityStatus = EFacilityStatus.Broked;
            }
            else
            {
                facilityStatus = EFacilityStatus.Disabled;
            }
        }

        public void Repair(int amount)
        {
            // TODO: 시설 수리 처리와 상태 복구 규칙을 구체화한다.
            totalDamage = Mathf.Max(totalDamage-amount, 0);

            maxPower = maxPowerLimit - totalDamage;
            UpdateFacilityStatus();
        }

        public int HandlePowerOverflow()
        {
            int overflowAmount = power - maxPower;
            if (overflowAmount < 0)
            {
                return 0;
            }

            UpdateFacilityStatus();
            return overflowAmount;
        }

    }
}
