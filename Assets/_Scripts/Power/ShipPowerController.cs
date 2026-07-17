using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    public class ShipPowerController : MonoBehaviour
    {
        [SerializeField] protected ReactorSystem reactorSystem;
        [SerializeField] protected List<FacilitySystem> facilities = new();
        protected readonly Dictionary<RoomType, int> assignedPowers = new();
        public int GetReactorMaxPower => reactorSystem.MaxPower;
        public int GetPower => reactorSystem.Power;

        #region Unity Method

        protected virtual void Start()
        {
            facilities.Add(GetComponent<ShieldSystem>());
            facilities.Add(GetComponent<WeaponSystem>());
            facilities.Add(GetComponent<EngineSystem>());
        }

        protected void Update()
        {
            for (RoomType type = RoomType.Weapon; type <= RoomType.Shield; type++)
            {
                HandleFacilityOverflow(type);
            }
        }

        #endregion

        #region Public Method

        public void RegisterFacility(FacilitySystem facility)
        {
            if (facility == null || facilities.Contains(facility) || assignedPowers.ContainsKey(facility.Type))
            {
                return;
            }

            facilities.Add(facility);

            assignedPowers.Add(facility.Type, facility.Power);
        }

        public bool TryAllocatePower(RoomType facilityType, int amount = 1)
        {
            if (amount <= 0 || reactorSystem == null)
            {
                return false;
            }

            FacilitySystem facility = FindFacility(facilityType);
            if (facility == null)
            {
                return false;
            }

            int currentPower = GetAssignedPower(facilityType);
            int nextPower = currentPower + amount;
            if (!CanSpendReactorPower(amount) || !facility.CanAddPower(amount) ||
                !CanSetFacilityPower(facility, nextPower))
            {
                return false;
            }

            SetAssignedPower(facilityType, nextPower);
            facility.AddPower(amount);
            reactorSystem.ReducePower(amount);
            return true;
        }

        public bool TryDeallocatePower(RoomType facilityType, int amount = 1)
        {
            if (amount <= 0 || reactorSystem == null)
            {
                return false;
            }

            FacilitySystem facility = FindFacility(facilityType);
            if (facility == null)
            {
                return false;
            }

            int currentFacilityPower = GetAssignedPower(facilityType);
            if (currentFacilityPower <= 0)
            {
                return false;
            }

            int recoverAmount = Mathf.Min(amount, currentFacilityPower);
            if (!CanRecoverReactorPower(recoverAmount))
            {
                return false;
            }

            int nextPower = currentFacilityPower - recoverAmount;
            SetAssignedPower(facilityType, nextPower);

            reactorSystem.AddPower(recoverAmount);
            facility.ReducePower(recoverAmount);
            return true;
        }

        public int GetAssignedPower(RoomType facilityType)
        {
            if (assignedPowers.TryGetValue(facilityType, out int power))
            {
                return power;
            }

            return 0;
        }

        public void AddReactorPower(int amount)
        {
            reactorSystem.AddMaxPower(amount);
        }

        public void SetReactorMaxPower(int amount)
        {
            reactorSystem.SetMaxPower(amount);
        }

        public void SetReactorPower(int amount)
        {
            reactorSystem.SetPower(amount);
        }

        protected FacilitySystem FindFacility(RoomType facilityType)
        {
            for (int i = 0; i < facilities.Count; i++)
            {
                if (facilities[i].Type == facilityType)
                {
                    return facilities[i];
                }
            }

            return null;
        }

        protected void SetAssignedPower(RoomType facilityType, int power)
        {
            assignedPowers[facilityType] = power;
        }

        protected bool CanSpendReactorPower(int amount)
        {
            return reactorSystem.Power >= amount;
        }

        protected bool CanRecoverReactorPower(int amount)
        {
            return reactorSystem.Power + amount <= reactorSystem.MaxPower;
        }

        protected bool CanSetFacilityPower(FacilitySystem facility, int power)
        {
            return power <= facility.MaxPower;
        }

        //private int GetTotalAssignedPower()
        //{
        //    int totalPower = 0;

        //    foreach (int power in assignedPowers.Values)
        //    {
        //        totalPower += power;
        //    }

        //    return totalPower;
        //}

        protected void HandleFacilityOverflow(RoomType facilityType)
        {
            FacilitySystem facility = FindFacility(facilityType);
            if (facility == null)
            {
                return;
            }

            int overflowAmount = facility.HandlePowerOverflow();

            if (overflowAmount > 0)
            {
                TryDeallocatePower(facilityType, overflowAmount);
            }
        }

        public void ApplyState(FacilityState facilityState)
        {
            RoomType type = facilityState.Type;
            int power = facilityState.Power;
            SetAssignedPower(type, power);
        }

        #endregion
    }
}
