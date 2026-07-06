using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utilities;

namespace FTL
{
    /// <summary>
    /// GameManager는 전투 종료, 게임 종료 같은 씬간 정보를 관리
    /// </summary>
    public class GameManager : SingletonBase<GameManager>
    {
        private const string BATTLE_SCENE = "BattleScene";
        private const string BATTLE_SCENE1 = "BattleScene1";
        private const string BATTLE_SCENE2 = "BattleScene2";
        private const string BATTLE_SCENE3 = "BattleScene3";
        private const string BATTLE_PREPARE = "BattlePrepare";
        private const string VICTORY_SCENE = "Victory";

        public bool IsGameContextEmpty => gameContext == null || gameContext.IsEmpty;
        public bool IsCombatPaused => isCombatPaused;
        public bool IsBattleStopped => isBattleStopped;
        public bool CanRunCombat => isCombatPaused == false && isBattleStopped == false;
        public PlayerController Player;
        [field: SerializeField] public EBattleResult BattleResult { get; set; }

        [field: SerializeField] public EnemyDatabase EnemyDatabase { get; private set; }
        [field: SerializeField] public WeaponDatabase WeaponDatabase { get; private set; }
        [SerializeField] private GameObject toggleMenuObject;
        [SerializeField] private GameContext gameContext;
        private MainUIController mainUIController;
        private bool isCombatPaused;
        private bool isBattleStopped;

        private void Start()
        {
            InitUIObject();
        }

        private void InitUIObject()
        {
            mainUIController = FindFirstObjectByType<MainUIController>();
            if (mainUIController == null)
            {
                return;
            }

            toggleMenuObject = mainUIController.transform.GetChild(1).gameObject;
        }

        private void Update()
        {
            if (SimpleInput.GetKeyDown(Key.Escape))
            {
                string sceneName = SceneManager.GetActiveScene().name;
                if (sceneName == BATTLE_PREPARE || IsBattleScene(sceneName))
                {
                    ToggleMenu(sceneName);
                }
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            isBattleStopped = false;
            SetCombatPaused(false);
            InitUIObject();
            CursorController.Instance.ResetMouseSprite();
            if (scene.name == BATTLE_SCENE)
            {
                BindLinearBattleScene();
            }
            else if (scene.name == BATTLE_SCENE1)
            {
                EEnemyType enemyType = EEnemyType.Enemy1;
                SceneBinder sceneBinder = FindFirstObjectByType<SceneBinder>();
                BackgroundController backgroundController = FindFirstObjectByType<BackgroundController>();

                gameContext.SetEnemyType(enemyType);
                sceneBinder.BindSceneObjects(gameContext);
                backgroundController.SetRandomBackground();
            }
            else if (scene.name == BATTLE_SCENE2)
            {
                EEnemyType enemyType = EEnemyType.Enemy2;
                SceneBinder sceneBinder = FindFirstObjectByType<SceneBinder>();
                BackgroundController backgroundController = FindFirstObjectByType<BackgroundController>();

                gameContext.SetEnemyType(enemyType);
                sceneBinder.BindSceneObjects(gameContext);
                backgroundController.SetRandomBackground();
            }
            else if (scene.name == BATTLE_SCENE3)
            {
                EEnemyType enemyType = EEnemyType.Enemy3;
                SceneBinder sceneBinder = FindFirstObjectByType<SceneBinder>();
                BackgroundController backgroundController = FindFirstObjectByType<BackgroundController>();

                gameContext.SetEnemyType(enemyType);
                sceneBinder.BindSceneObjects(gameContext);
                backgroundController.SetRandomBackground();
            }
            else if (scene.name == BATTLE_PREPARE)
            {
                ShopManager shopManager = FindFirstObjectByType<ShopManager>();
                shopManager.ApplyGameContext(gameContext);
            }
        }

        private void BindLinearBattleScene()
        {
            int battlePrepareVisitCount = ShopManager.BattlePrepareVisitCount;
            EEnemyType enemyType = EEnemyType.Enemy1;
            bool isAsteroidStage = false;
            bool isSunStage = false;

            if (battlePrepareVisitCount == 1)
            {
                enemyType = EEnemyType.Enemy2;
                isAsteroidStage = true;
            }
            else if (battlePrepareVisitCount == 2)
            {
                enemyType = EEnemyType.Enemy2;
                isSunStage = true;
            }
            else if (battlePrepareVisitCount == 3)
            {
                enemyType = EEnemyType.Enemy3;
                isAsteroidStage = true;
                isSunStage = true;
            }
            else if (battlePrepareVisitCount >= 4)
            {
                SceneManager.LoadScene(VICTORY_SCENE);
                return;
            }

            SceneBinder sceneBinder = FindFirstObjectByType<SceneBinder>();
            BackgroundController backgroundController = FindFirstObjectByType<BackgroundController>();

            gameContext.SetEnemyType(enemyType);
            sceneBinder.BindSceneObjectsWithEnvironment(gameContext, isAsteroidStage, isSunStage);
            backgroundController.SetRandomBackground();
        }

        public void HandleBattleResult(EBattleResult battleResult)
        {
            StopBattle();
            // 함선이 파괴되면
            switch (battleResult)
            {
                case EBattleResult.PlayerShipDestroyed:
                    Debug.Log("Player ship destroyed");
                    mainUIController.SetBattleResult(battleResult, 0);
                    break;
                case EBattleResult.PlayerAllCrewDead:
                    // 게임오버 화면으로
                    // 어떤 Uicontroller인지 몰라도 얘한테 시켜서 게임오버화면으로 이동
                    mainUIController.SetBattleResult(battleResult, 0);
                    Debug.Log("Player all crew dead.");
                    break;
                case EBattleResult.EnemyShipDestroyed:
                    SetGameContext(Player);
                    gameContext.AddScrap(100);
                    mainUIController.SetBattleResult(battleResult, 100);
                    Debug.Log("Enemy ship destroyed.");
                    break;
                case EBattleResult.EnemyAllCrewDead:
                    SetGameContext(Player);
                    gameContext.AddScrap(200);
                    mainUIController.SetBattleResult(battleResult, 200);
                    Debug.Log("Enemy all crew dead.");
                    break;
                case EBattleResult.PlayerRun:
                    // 전투준비 화면으로 이동
                    // 어떤 Uicontroller인지 몰라도 얘한테 시켜서 전투준비화면으로 이동
                    SetGameContext(Player);
                    mainUIController.SetBattleResult(battleResult, 0);
                    Debug.Log("Player run.");
                    break;
                case EBattleResult.None:
                default:
                    Debug.LogError("Unknown EBattleResult");
                    break;
            }
        }

        public void MoveSceneByBattleResult(EBattleResult battleResult) 
        {
            switch (battleResult)
            {
                case EBattleResult.PlayerShipDestroyed:
                    mainUIController.MoveScene(ESceneIndex.GameOver);
                    break;
                case EBattleResult.PlayerAllCrewDead:
                    mainUIController.MoveScene(ESceneIndex.GameOver);
                    Debug.Log("Player all crew dead.");
                    break;
                case EBattleResult.EnemyShipDestroyed:
                    mainUIController.MoveScene(ESceneIndex.BattlePrepare);
                    Debug.Log("Enemy ship destroyed.");
                    break;
                case EBattleResult.EnemyAllCrewDead:
                    mainUIController.MoveScene(ESceneIndex.BattlePrepare);
                    Debug.Log("Enemy all crew dead.");
                    break;
                case EBattleResult.PlayerRun:
                    mainUIController.MoveScene(ESceneIndex.BattlePrepare);
                    Debug.Log("Player run.");
                    break;
                case EBattleResult.None:
                default:
                    Debug.LogError("Unknown EBattleResult");
                    break;
            }
        }

        public void PlayerRun()
        {
            HandleBattleResult(EBattleResult.PlayerRun);
        }

        public void SetCombatPaused(bool isPaused)
        {
            if (isBattleStopped && isPaused)
            {
                return;
            }

            isCombatPaused = isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
        }

        public void StopBattle()
        {
            isBattleStopped = true;
            if (toggleMenuObject != null)
            {
                toggleMenuObject.SetActive(false);
            }

            SetCombatPaused(false);
        }

        private void ToggleMenu(string sceneName)
        {
            if (isBattleStopped)
            {
                return;
            }

            if (toggleMenuObject == null)
            {
                Debug.LogError("GameManager: toggleMenuObject is not assigned.");
                return;
            }

            bool isToggleMenuActive = toggleMenuObject.activeSelf;
            bool shouldOpenMenu = isToggleMenuActive == false;
            toggleMenuObject.SetActive(shouldOpenMenu);

            if (IsBattleScene(sceneName))
            {
                SetCombatPaused(shouldOpenMenu);
            }
        }

        private bool IsBattleScene(string sceneName)
        {
            return sceneName == BATTLE_SCENE ||
                sceneName == BATTLE_SCENE1 ||
                sceneName == BATTLE_SCENE2 ||
                sceneName == BATTLE_SCENE3;
        }

        //public void SetGameContext(List<Room> rooms, List<FacilitySystem> facilities, List<CrewController> crews, int shipHp, int shipMaxHp, int scrapAmount, int shipReactorCapacity, int shipPower)
        //{
        //    if (gameContext == null)
        //    {
        //        gameContext = new GameContext(rooms, facilities, crews, scrapAmount, shipHp, shipMaxHp, shipReactorCapacity, shipPower);
        //    }
        //    else
        //    {
        //        gameContext.SetGameContext(rooms, facilities, crews, scrapAmount, shipHp, shipMaxHp, shipReactorCapacity, shipPower);
        //    }
        //}

        public void SetGameContext(PlayerController player)
        {
            List<Room> rooms = player.Rooms;
            List<FacilitySystem> facilities = player.FacilitySystems;
            List<CrewController> crews = player.Crews;

            List<FacilityState> facilityStates = new List<FacilityState>();
            for (int i = 0; i < facilities.Count; i++)
            {
                facilityStates.Add(new FacilityState(facilities[i]));
            }

            List<CrewState> crewStates = new List<CrewState>();
            for (int i = 0; i < crews.Count; i++)
            {
                crewStates.Add(new CrewState(crews[i]));
            }

            gameContext = new GameContext(rooms, facilityStates, crewStates, player.ScrapAmount, player.Hp, player.ShipData.MaxHp, player.PowerSystem.GetReactorMaxPower, player.PowerSystem.GetPower);
            gameContext.SetWeaponTypes(player.WeaponTypes);
        }

        public void EnsureBattlePrepareTestGameContext()
        {
            if (IsGameContextEmpty == false)
            {
                return;
            }

            List<Room> rooms = new List<Room>();

            List<FacilityState> facilityStates = new List<FacilityState>
            {
                new FacilityState(0, 3, 1, ERoomType.Weapon),
                new FacilityState(0, 3, 1, ERoomType.Engine),
                new FacilityState(0, 3, 2, ERoomType.Shield)
            };

            List<CrewState> crewStates = new List<CrewState>
            {
                new CrewState(100, 100, "Hans", 1),
                new CrewState(100, 100, "Tom", 2),
                new CrewState(100, 100, "Jane", 3)
            };

            gameContext = new GameContext(rooms, facilityStates, crewStates, 100, 30, 30, 6, 2);
        }

        //public void AddScrap(int amount = 1)
        //{
        //    if (gameContext == null)
        //    {
        //        Debug.LogError("GameContext is null. Cannot add scrap.");
        //        return;
        //    }
        //    gameContext.ScrapAmount += amount;
        //}

        public int GetScrap()
        {
            if (gameContext == null)
            {
                Debug.LogError("GameContext is null. Cannot get scrap.");
                return 0;
            }
            return gameContext.ScrapAmount;
        }

        public void IncreaseMaxPowerLimitOfFacility(ERoomType roomType, int amount = 1)
        {
            gameContext.IncreaseMaxPowerLimitOfFacility(roomType, amount);
        }

        public void ResetGameContext()
        {
            gameContext = null;
            ShopManager.BattlePrepareVisitCount = 0;
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void IncreaseReactorMaxPower(int amount = 1)
        {
            gameContext.IncreaseReactorMaxPower(amount);
        }

        public int GetCrewHp(int crewIndex)
        {
            return gameContext.GetCrewHp(crewIndex);
        }

        public int GetCrewMaxHp(int crewIndex)
        {
            return gameContext.GetCrewMaxHp(crewIndex);
        }

        public int GetCrewCount()
        {
            return gameContext.GetCrewCount();
        }

        public string GetCrewName(int crewIndex)
        {
            return gameContext.GetCrewName(crewIndex);
        }

        public void AddCrewHp(int crewIndex, int realIncrease)
        {
            gameContext.AddCrewHp(crewIndex, realIncrease);
        }

        public int GetShipHp()
        {
            return gameContext.ShipHp;
        }

        public int GetShipMaxHp()
        {
            return gameContext.ShipMaxHp;
        }

        public int GetShipReactorCapacity()
        {
            return gameContext.ShipReactorCapacity;
        }

        public int GetFacilityMaxPowerLimit(ERoomType roomType)
        {
            return gameContext.GetFacilityMaxPowerLimit(roomType);
        }

        public void AddShipHp(int amount)
        {
            gameContext.AddShipHp(amount);
        }
        #region Test Method
        public void TestHandleBattleResult()
        {
            if (SimpleInput.GetKeyDown(Key.Digit1))
            {
                BattleResult = EBattleResult.PlayerShipDestroyed;
            }

            if (SimpleInput.GetKeyDown(Key.Digit2))
            {
                BattleResult = EBattleResult.PlayerAllCrewDead;
            }

            if (SimpleInput.GetKeyDown(Key.Digit3))
            {
                BattleResult = EBattleResult.PlayerRun;
            }

            if (SimpleInput.GetKeyDown(Key.Digit4))
            {
                BattleResult = EBattleResult.EnemyAllCrewDead;
            }

            if (SimpleInput.GetKeyDown(Key.Digit5))
            {
                BattleResult = EBattleResult.EnemyShipDestroyed;
            }

            if (SimpleInput.AnyKeyDown())
            {
                HandleBattleResult(BattleResult);
            }
        }

        public void TurnOffToggleMenu()
        {
            if (toggleMenuObject != null)
            {
                toggleMenuObject.SetActive(false);
            }

            SetCombatPaused(false);
        }

        public void ReduceScrap(int v)
        {
            gameContext.ReduceScrap(v);
        }

        [ContextMenu("AddScrap100")]
        public void AddScrap100()
        {
            gameContext.AddScrap(100);
        }

        [ContextMenu("Change Background")]
        public void ChangeBackground()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == BATTLE_SCENE)
            {
                BackgroundController backgroundController = FindFirstObjectByType<BackgroundController>();
                backgroundController.SetRandomBackground();
            }

        }
        #endregion
    }
}
