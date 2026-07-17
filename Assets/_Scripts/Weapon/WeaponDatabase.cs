using System;
using UnityEngine;

namespace FTL
{
    [Serializable]
    public class WeaponPrefabEntry
    {
        public WeaponType weaponType;
        public GameObject prefab;
    }

    [CreateAssetMenu(fileName = "WeaponDatabase", menuName = "SO/WeaponDatabase")]
    public class WeaponDatabase : ScriptableObject
    {
        [SerializeField] private WeaponPrefabEntry[] entries;

        #region Public Method

        public GameObject GetPrefab(WeaponType weaponType)
        {
            foreach (WeaponPrefabEntry entry in entries)
            {
                if (entry.weaponType == weaponType)
                {
                    return entry.prefab;
                }
            }

            return null;
        }

        #endregion
    }
}
