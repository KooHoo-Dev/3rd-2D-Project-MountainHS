using System;
using UnityEngine;

namespace FTL
{
    [Serializable]
    public class PowerData
    {
        public int ShieldPower;
        public int WeaponPower;
        public int EnginePower;
    }

    public class EnemyPowerController: ShipPowerController
    {
        [SerializeField] private PowerData powerData;

        #region Unity Method

        protected override void Start()
        {
            base.Start();
            // 실드 2칸
            TryAllocatePower(ERoomType.Shield, powerData.ShieldPower);
            // 무기 2칸
            TryAllocatePower(ERoomType.Weapon, powerData.WeaponPower);
            // 엔진 1칸
            TryAllocatePower(ERoomType.Engine, powerData.EnginePower);
            
        }

        #endregion
    }
}
