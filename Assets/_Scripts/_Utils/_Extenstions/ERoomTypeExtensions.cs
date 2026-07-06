using System;

namespace FTL
{
    public static class ERoomTypeExtensions
    {
        public static string ToRoomTypeName(this ERoomType roomType)
        {
            switch (roomType)
            {
                case ERoomType.Weapon:
                    return "Weapon";
                case ERoomType.Engine:
                    return "Engine";
                case ERoomType.Shield:
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
