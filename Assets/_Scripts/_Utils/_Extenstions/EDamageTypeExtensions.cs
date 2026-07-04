using UnityEngine;

namespace FTL
{
    public static class EDamageTypeExtensions
    {
        public static int GetDamageFromDamageType(this EDamageType type, ProjectileController projectile)
        {
            switch (type)
            {
                case EDamageType.Shield:
                    return projectile.ShieldDamage;
                case EDamageType.Room:
                    return projectile.Damage;
                case EDamageType.Crew:
                    return projectile.CrewDamage;
                default:
                    Debug.LogError($"Unknown damage type: {type}");
                    return -1;
            }
        }

        public static Color GetColorFromDamageType(this EDamageType type)
        {
            switch (type)
            {
                case EDamageType.Shield:
                    return Color.skyBlue;
                case EDamageType.Room:
                    return Color.white;
                case EDamageType.Crew:
                    return Color.orange;
                default:
                    Debug.LogError($"Unknown damage type: {type}");
                    return Color.white;
            }
        }
    }
}
