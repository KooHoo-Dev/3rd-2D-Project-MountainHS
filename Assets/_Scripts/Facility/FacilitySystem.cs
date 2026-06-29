using Unity.VisualScripting;
using UnityEngine;

namespace FTL
{
    public class FacilitySystem : MonoBehaviour
    {
        public ERoomType Type => type;
        public EFacilityStatus FacilityStatus => facilityStatus;
        public int MaxPower => maxPower;
        public bool IsPowered => Power > 0;
        [field: SerializeField] public bool IsAssisted { get; protected set; } = false;
        [field: SerializeField] public float RepairDuration { get; set; } = 7.5f;
        [field: SerializeField] public float RepairProgress { get; set; } = 0f;
        [field: SerializeField] public int MaxPowerLimit { get; set; } = 3;
        public int TotalDamage;
        public int Power;
        [SerializeField] protected ERoomType type;
        [SerializeField] protected int maxPower;
        protected EFacilityStatus facilityStatus;

        protected virtual void Awake()
        {
            IsAssisted = false;
            Power = 0;
            maxPower = MaxPowerLimit;
            facilityStatus = EFacilityStatus.Normal;
            TotalDamage = 0;
            UpdateFacilityStatus();
        }

        public bool CanAddPower(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            return Power + amount <= maxPower;
        }

        public void AddPower(int amount)
        {
            if (CanAddPower(amount))
            {
                Power += amount;
            }
            UpdateFacilityStatus();
        }

        public void ReducePower(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Power = Mathf.Max(Power - amount, 0);
            UpdateFacilityStatus();
        }

        public virtual void TakeDamage(int amount)
        {
            int newDamage = TotalDamage + amount;
            newDamage = Mathf.Min(newDamage, MaxPowerLimit);
            TotalDamage = newDamage;
            maxPower = MaxPowerLimit - newDamage;

            UpdateFacilityStatus();
        }

        public void Repair(int amount)
        {
            TotalDamage = Mathf.Max(TotalDamage-amount, 0);

            maxPower = MaxPowerLimit - TotalDamage;
            UpdateFacilityStatus();
        }

        public void UpdateFacilityStatus()
        {
            if (TotalDamage == MaxPowerLimit)
            {
                facilityStatus = EFacilityStatus.Broked;
            }
            else if (TotalDamage > 0 && TotalDamage < MaxPowerLimit)
            {
                facilityStatus = EFacilityStatus.Damaged;
            }
            else if (Power == 0)
            {
                facilityStatus = EFacilityStatus.Disabled;
            }
            else if (TotalDamage == 0)
            {
                facilityStatus = EFacilityStatus.Normal;
            }
        }

        public int HandlePowerOverflow()
        {
            int overflowAmount = Power - maxPower;
            if (overflowAmount < 0)
            {
                return 0;
            }

            return overflowAmount;
        }

        public virtual void ApplyAssistEffectOnFacility(bool isApplied)
        {
            if (isApplied != IsAssisted)
            {
                Debug.Log($"Apply crew assist! in {this}! before: {IsAssisted} after: {isApplied}");
                IsAssisted = isApplied;
            }
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
