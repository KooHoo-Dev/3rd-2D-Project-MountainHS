namespace FTL
{
    public static class ERoomTypeExtensions
    {
        public static string ToRoomTypeName(this RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Weapon:
                    return "Weapon";
                case RoomType.Engine:
                    return "Engine";
                case RoomType.Shield:
                    return "Shield";
                default:
                    return string.Empty;
            }
        }
        //public static Type ToSystemType(this ERoomType type) => type switch
        //{
        //    ERoomType.Weapon => typeof(WeaponSystem),
        //    ERoomType.Engine => typeof(EngineSystem),
        //    ERoomType.Shield => typeof(ShieldSystem),
        //    _ => typeof(FacilitySystem)
        //};
    }
}
