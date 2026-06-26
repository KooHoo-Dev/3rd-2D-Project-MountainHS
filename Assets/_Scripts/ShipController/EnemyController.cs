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
        }

        public void TestAllocateShieldPower()
        {
            RequestAllocatePower(ERoomType.Shield);
        }

        public void TestDeallocateShieldPower()
        {
            RequestDeallocatePower(ERoomType.Shield);
        }

        public override IEnumerator DeadCoroutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            GameManager.Instance.HandleBattleResult(EBattleResult.EnemyShipDestroyed);
            Destroy(gameObject);
        }
    }
}
