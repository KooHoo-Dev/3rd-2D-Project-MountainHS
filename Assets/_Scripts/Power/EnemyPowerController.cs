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
        protected override void Start()
        {
            base.Start();
            // 실드 2칸
            TryAllocatePower(RoomType.Shield, powerData.ShieldPower);
            // 무기 2칸
            TryAllocatePower(RoomType.Weapon, powerData.WeaponPower);
            // 엔진 1칸
            TryAllocatePower(RoomType.Engine, powerData.EnginePower);
            
        }
    }
}
