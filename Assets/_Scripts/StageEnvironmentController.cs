using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    public class StageEnvironmentController : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float effectInterval = 10f;

        [Header("Room Damage")]
        [SerializeField] private int minRoomHitCount = 1;
        [SerializeField] private int maxRoomHitCount = 2;
        [SerializeField] private int roomDamage = 1;
        [SerializeField] private int crewDamage = 10;
        [SerializeField] private bool applyToPlayer = true;
        [SerializeField] private bool applyToEnemy = true;

        [Header("Asteroid Object")]
        [SerializeField] private GameObject asteroidAttackPrefab;
        [SerializeField] private Transform asteroidGroupTransform;
        [SerializeField] private float asteroidLifetime = 1.5f;

        private CombatSystem combatSystem;
        private Coroutine environmentCoroutine;
        private bool hasLoggedMissingAsteroidPrefab;

        public void InitStageEnvironment(CombatSystem combatSystem)
        {
            this.combatSystem = combatSystem;
            hasLoggedMissingAsteroidPrefab = false;
            StartEnvironmentEffects();
        }

        private void OnDisable()
        {
            StopEnvironmentEffects();
        }

        private void StartEnvironmentEffects()
        {
            StopEnvironmentEffects();

            if (combatSystem == null)
            {
                Debug.LogError("StageEnvironmentController: CombatSystem is not assigned.");
                return;
            }

            if (effectInterval <= 0f)
            {
                Debug.LogError("StageEnvironmentController: effectInterval must be greater than 0.");
                return;
            }

            environmentCoroutine = StartCoroutine(EnvironmentCoroutine());
        }

        private void StopEnvironmentEffects()
        {
            if (environmentCoroutine == null)
            {
                return;
            }

            StopCoroutine(environmentCoroutine);
            environmentCoroutine = null;
        }

        private IEnumerator EnvironmentCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(effectInterval);
                ApplyEnvironmentEffect();
            }
        }

        private void ApplyEnvironmentEffect()
        {
            if (applyToPlayer)
            {
                ApplyEnvironmentEffectToShip(combatSystem.GetShip(EShipSide.Player));
            }

            if (applyToEnemy)
            {
                ApplyEnvironmentEffectToShip(combatSystem.GetShip(EShipSide.Enemy0));
            }
        }

        private void ApplyEnvironmentEffectToShip(ShipController ship)
        {
            if (ship == null || ship.IsDead)
            {
                return;
            }

            List<Room> rooms = ship.Rooms;
            if (rooms == null || rooms.Count == 0)
            {
                Debug.LogError($"{ship.name}: Cannot apply environment damage. Rooms are empty.");
                return;
            }

            int minHitCount = Mathf.Max(1, minRoomHitCount);
            int maxHitCount = Mathf.Max(minHitCount, maxRoomHitCount);
            int roomHitCount = Random.Range(minHitCount, maxHitCount + 1);
            roomHitCount = Mathf.Clamp(roomHitCount, 1, rooms.Count);

            List<int> selectedIndexes = new List<int>();
            for (int i = 0; i < roomHitCount; i++)
            {
                int roomIndex = GetRandomRoomIndex(rooms.Count, selectedIndexes);
                selectedIndexes.Add(roomIndex);

                Room room = rooms[roomIndex];
                SpawnAsteroidAttack(room);
                ship.TakeDamageToRoom(roomDamage, crewDamage, room.Id);
            }
        }

        private int GetRandomRoomIndex(int roomCount, List<int> selectedIndexes)
        {
            for (int i = 0; i < 20; i++)
            {
                int randomIndex = Random.Range(0, roomCount);
                if (selectedIndexes.Contains(randomIndex) == false)
                {
                    return randomIndex;
                }
            }

            for (int i = 0; i < roomCount; i++)
            {
                if (selectedIndexes.Contains(i) == false)
                {
                    return i;
                }
            }

            return 0;
        }

        private void SpawnAsteroidAttack(Room room)
        {
            if (asteroidAttackPrefab == null)
            {
                if (hasLoggedMissingAsteroidPrefab == false)
                {
                    Debug.LogError("StageEnvironmentController: asteroidAttackPrefab is not assigned.");
                    hasLoggedMissingAsteroidPrefab = true;
                }

                return;
            }

            GameObject asteroidObject = Instantiate(asteroidAttackPrefab, room.GlobalPosition, Quaternion.identity, asteroidGroupTransform);
            if (asteroidLifetime > 0f)
            {
                Destroy(asteroidObject, asteroidLifetime);
            }
        }
    }
}
