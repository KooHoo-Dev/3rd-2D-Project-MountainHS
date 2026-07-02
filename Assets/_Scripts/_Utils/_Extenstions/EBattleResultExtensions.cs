namespace FTL
{
    public static class EBattleResultExtensions
    {
        public static string GetBattleResultName(this EBattleResult resultType)
        {
            switch (resultType)
            {
                case EBattleResult.EnemyAllCrewDead:
                    return "Victory: All Crew Dead";
                case EBattleResult.PlayerShipDestroyed:
                    return "Defeat: Ship Destroyed";
                case EBattleResult.EnemyShipDestroyed:
                    return "Victory: Ship Destroyed";
                case EBattleResult.PlayerRun:
                    return "Escaped";
                case EBattleResult.PlayerAllCrewDead:
                    return "Defeat: All Crew Dead";
                case EBattleResult.None:
                default:
                    return string.Empty;
            }
        }
    }
}
