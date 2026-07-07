namespace FTL
{
    public static class EBattleResultExtensions
    {
        public static string GetBattleResultName(this BattleResult resultType)
        {
            switch (resultType)
            {
                case BattleResult.EnemyAllCrewDead:
                    return "Victory: All Crew Dead";
                case BattleResult.PlayerShipDestroyed:
                    return "Defeat: Ship Destroyed";
                case BattleResult.EnemyShipDestroyed:
                    return "Victory: Ship Destroyed";
                case BattleResult.PlayerRun:
                    return "Escaped";
                case BattleResult.PlayerAllCrewDead:
                    return "Defeat: All Crew Dead";
                case BattleResult.None:
                default:
                    return string.Empty;
            }
        }
    }
}
