namespace FTL
{
    public class EnemyPowerController: ShipPowerController
    {
        protected override void Start()
        {
            base.Start();
            // 실드 2칸
            TryAllocatePower(ERoomType.Shield, 2);
            // 무기 2칸
            TryAllocatePower(ERoomType.Weapon, 2);
            // 엔진 1칸
            TryAllocatePower(ERoomType.Engine, 1);
            
        }
    }
}
