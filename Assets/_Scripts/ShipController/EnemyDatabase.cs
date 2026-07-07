using UnityEngine;

namespace FTL
{
    [System.Serializable]
    public class EnemyPrefabEntry
    {
        public EnemyType enemyType;
        public GameObject prefab;
    }

    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "SO/EnemyDatabase")]
    public class EnemyDatabase : ScriptableObject
    {
        [SerializeField] private EnemyPrefabEntry[] entries;

        public GameObject GetPrefab(EnemyType enemyType)
        {
            if (entries == null)
            {
                return null;
            }

            if (enemyType == EnemyType.None)
            {
                return null;
            }

            if (enemyType == EnemyType.Random)
            {
                return GetRandomPrefab();
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] == null)
                {
                    continue;
                }

                if (entries[i].enemyType == enemyType)
                {
                    return entries[i].prefab;
                }
            }

            return null;
        }

        public GameObject GetRandomPrefab()
        {
            if (entries == null || entries.Length == 0)
            {
                return null;
            }

            int validEntryCount = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] != null && entries[i].prefab != null)
                {
                    validEntryCount++;
                }
            }

            if (validEntryCount == 0)
            {
                return null;
            }

            int randomIndex = Random.Range(0, validEntryCount);
            int currentIndex = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i] == null || entries[i].prefab == null)
                {
                    continue;
                }

                if (currentIndex == randomIndex)
                {
                    return entries[i].prefab;
                }

                currentIndex++;
            }

            return null;
        }
    }
}
