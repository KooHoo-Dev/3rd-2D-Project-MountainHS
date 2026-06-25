using System.Collections;
using UnityEngine;

namespace FTL
{
    public class EnemyController: ShipController, IDamageable
    {
        private void Update()
        {
        }

        public void TestAllocateShieldPower()
        {
            RequestAllocatePower(ERoomType.Shield);
        }

        public void TestDeallocateShieldPower()
        {
            RequestDeallocatePower(ERoomType.Shield);
        }

    }
}
