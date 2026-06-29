using System.Collections;
using UnityEngine;

namespace FTL
{
    public class EnemyController: ShipController    
    {
        private ITargeting targetingSystem;

        private void Start()
        {
            targetingSystem = new RandomTargeter(CombatSystem.Instance.GetPlayerRooms());
            weaponSystem.TargetingSystem = targetingSystem;
            Ownership.ShipSide = EShipSide.Enemy0;
        }
    }
}
