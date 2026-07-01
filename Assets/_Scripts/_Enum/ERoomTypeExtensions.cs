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
    }
}
