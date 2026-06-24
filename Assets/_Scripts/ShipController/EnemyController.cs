namespace FTL
{
    public class EnemyController: ShipController, IDamageable
    {
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
