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

    public class EnemyPowerController : ShipPowerController
    {
        [SerializeField] private PowerData powerData;

        #region Unity Method

        protected override void Start()
        {
            base.Start();
            // 실드 2칸
            AllocatePower(RoomType.Shield, powerData.ShieldPower);
            // 무기 2칸
            AllocatePower(RoomType.Weapon, powerData.WeaponPower);
            // 엔진 1칸
            AllocatePower(RoomType.Engine, powerData.EnginePower);
        }

        #endregion
    }
}
