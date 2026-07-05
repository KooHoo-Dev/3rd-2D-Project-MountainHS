using UnityEngine;

namespace FTL
{
    public class SceneBinder : MonoBehaviour
    {
        [Header("Enemy Spawn")]
        [SerializeField] private EnemyDatabase enemyDatabase;
        [SerializeField] private Transform enemySpawnPoint;
        [SerializeField] private Transform enemySpawnParent;
        [SerializeField] private Transform enemyCrewSummaryGroupTransform;
        [SerializeField] private Transform enemyWeaponChargeGroupTransform;

        [Header("Stage Environment")]
        [SerializeField] private StageEnvironmentController stageEnvironmentController;

        public void BindSceneObjects(GameContext gameContext)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            EnemyController enemy = SpawnRandomEnemyOrFindSceneEnemy();
            if (player == null)
            {
                Debug.LogError("SceneBinder: PlayerController was not found.");
                return;
            }

            if (enemy == null)
            {
                Debug.LogError("SceneBinder: EnemyController was not found.");
                return;
            }

            if (gameContext == null)
            {
                Debug.LogError("SceneBinder: GameContext is null.");
                return;
            }

            player.Hp = gameContext.ShipHp;
            player.ScrapAmount = gameContext.ScrapAmount;
            player.SetReactorMaxPower(gameContext.ShipReactorCapacity);
            player.SetReactorPower(gameContext.ShipPower);
            player.SetFacilitySystems(gameContext.FacilityStates);
            player.SetCrewsState(gameContext.CrewStates);
            player.SetWeaponTypes(gameContext.WeaponTypes);
            BindCombatSystem(player, enemy);
            BindStageEnvironment();
        }

        private void BindCombatSystem(PlayerController player, EnemyController enemy)
        {
            CombatSystem.Instance.InitCombatSystem(player, enemy);
        }

        private void BindStageEnvironment()
        {
            if (stageEnvironmentController == null)
            {
                return;
            }

            stageEnvironmentController.InitStageEnvironment(CombatSystem.Instance);
        }

        private EnemyController SpawnRandomEnemyOrFindSceneEnemy()
        {
            if (enemyDatabase == null)
            {
                Debug.LogWarning("SceneBinder: EnemyDatabase is not assigned. Using scene enemy instead.");
                return FindFirstObjectByType<EnemyController>();
            }

            GameObject enemyPrefab = enemyDatabase.GetRandomPrefab();
            if (enemyPrefab == null)
            {
                Debug.LogError("SceneBinder: Enemy prefab was not found in EnemyDatabase.");
                return FindFirstObjectByType<EnemyController>();
            }

            ClearSceneEnemies();
            ClearEnemyUIGroup(enemyCrewSummaryGroupTransform);
            ClearEnemyUIGroup(enemyWeaponChargeGroupTransform);

            Vector3 spawnPosition = Vector3.zero;
            Quaternion spawnRotation = Quaternion.identity;
            if (enemySpawnPoint != null)
            {
                spawnPosition = enemySpawnPoint.position;
                spawnRotation = enemySpawnPoint.rotation;
            }

            GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, spawnRotation, enemySpawnParent);
            EnemyController enemy = enemyObject.GetComponent<EnemyController>();
            if (enemy == null)
            {
                Debug.LogError("SceneBinder: Spawned enemy prefab does not have EnemyController.");
                return null;
            }

            enemy.SetExternalUIGroups(enemyCrewSummaryGroupTransform, enemyWeaponChargeGroupTransform);
            enemy.InitializeShipIfReady();
            return enemy;
        }

        private void ClearSceneEnemies()
        {
            EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i].gameObject.SetActive(false);
                Destroy(enemies[i].gameObject);
            }
        }

        private void ClearEnemyUIGroup(Transform groupTransform)
        {
            if (groupTransform == null)
            {
                return;
            }

            for (int i = groupTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(groupTransform.GetChild(i).gameObject);
            }
        }

    }
}
