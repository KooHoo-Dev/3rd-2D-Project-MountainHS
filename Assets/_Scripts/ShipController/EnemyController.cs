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
            weaponSystem.SetWeaponsTargetingSystem(targetingSystem);
            Ownership.ShipSide = ShipSide.Enemy0;
        }

    }
}
