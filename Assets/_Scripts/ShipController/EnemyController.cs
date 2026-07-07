using System.Collections;
using UnityEngine;

namespace FTL
{
    public class EnemyController: ShipController    
    {
        private ITargeting targetingSystem;

        #region Unity Method

        private void Start()
        {
            targetingSystem = new RandomTargeter(CombatSystem.Instance.GetPlayerRooms());
            weaponSystem.SetWeaponsTargetingSystem(targetingSystem);
            Ownership.ShipSide = EShipSide.Enemy0;
        }

        #endregion
    }
}
