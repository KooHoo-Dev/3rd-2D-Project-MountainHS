using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    public class ShipPowerController : MonoBehaviour
    {
        [SerializeField] protected ReactorSystem reactorSystem;
        [SerializeField] protected List<FacilitySystem> facilities = new List<FacilitySystem>();
        protected readonly Dictionary<ERoomType, int> assignedPowers = new Dictionary<ERoomType, int>();

        protected virtual void Start()
        {
            facilities.Add(GetComponent<ShieldSystem>());
            facilities.Add(GetComponent<WeaponSystem>());
            facilities.Add(GetComponent<EngineSystem>());
        }
        protected void Update()
        {
            for (ERoomType type = ERoomType.Weapon; type <= ERoomType.Shield; type++)
            {
                HandleFacilityOverflow(type);
            }
        }

        public void RegisterFacility(FacilitySystem facility)
        {
            if (facility == null || facilities.Contains(facility) || assignedPowers.ContainsKey(facility.Type))
            {
                return;
            }

            facilities.Add(facility);

            assignedPowers.Add(facility.Type, facility.Power);
        }

        public bool TryAllocatePower(ERoomType facilityType, int amount = 1)
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
            if (CanSpendReactorPower(amount) == false || facility.CanAddPower(amount) == false || CanSetFacilityPower(facility, nextPower) == false)
            {
                return false;
            }

            SetAssignedPower(facilityType, nextPower);
            facility.AddPower(amount);
            reactorSystem.ReducePower(amount);
            return true;
        }

        public bool TryDeallocatePower(ERoomType facilityType, int amount = 1)
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
            if (CanRecoverReactorPower(recoverAmount) == false)
            {
                return false;
            }

            int nextPower = currentFacilityPower - recoverAmount;
            SetAssignedPower(facilityType, nextPower);

            reactorSystem.AddPower(recoverAmount);
            facility.ReducePower(recoverAmount);
            return true;
        }

        public int GetAssignedPower(ERoomType facilityType)
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

        protected FacilitySystem FindFacility(ERoomType facilityType)
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

        protected void SetAssignedPower(ERoomType facilityType, int power)
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

        protected void HandleFacilityOverflow(ERoomType facilityType)
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

    }
}
