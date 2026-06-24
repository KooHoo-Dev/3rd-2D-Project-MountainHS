using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace FTL
{
    public class ShipController : MonoBehaviour, IDamageable
    {
        [field: SerializeField] public ShipData ShipData { get; private set; }
        [field: SerializeField] public int Hp { get; private set; }

        [SerializeField] protected ShipPowerController powerSystem;

        protected void Awake()
        {
            Hp = ShipData.MaxHp;
        }

        protected void HandlePowerInput(ERoomType facilityType, bool isRecoverInput)
        {
            if (isRecoverInput)
            {
                RequestDeallocatePower(facilityType);
            }
            else
            {
                RequestAllocatePower(facilityType);
            }
        }

        public void TakeDamage(int damage)
        {
            Hp = (Hp - damage < 0) ? 0 : Hp - damage;
        }

        public void RestoreHp(int damage)
        {
            Hp = (Hp + damage > ShipData.MaxHp) ? ShipData.MaxHp : Hp + damage;
        }

        public void RequestAllocatePower(ERoomType facilityType)
        {
            powerSystem.TryAllocatePower(facilityType);
        }

        public void RequestDeallocatePower(ERoomType facilityType)
        {
            powerSystem.TryDeallocatePower(facilityType);
        }

    }
}
