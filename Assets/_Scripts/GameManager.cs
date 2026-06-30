using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;

namespace FTL
{
    /// <summary>
    /// GameManager는 전투 종료, 게임 종료 같은 씬간 정보를 관리
    /// </summary>
    public class GameManager : SingletonBase<GameManager>
    {
        private const string MAINMENU_SCENE = "_MainMenu";
        private const string BATTLE_SCENE = "BattleScene";
        private const string GAMEOVER_SCENE = "GameOver";
        private const string BATTLE_PREPARE = "BattlePrepare";
        private const string MAIN_UI_TAG = "Main UI";

        public bool IsGameContextEmpty => gameContext == null;
        public PlayerController Player;
        [field: SerializeField] public EBattleResult BattleResult { get; set; }

        [SerializeField] private GameObject mainUIRoot;
        [SerializeField] private GameObject buttonGroupObject;
        [SerializeField] private GameObject toggleMenuObject;
        [SerializeField] private Button startButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button nextBattleButton;
        private MainUIController mainUIController;
        private List<string> sceneNameList = new List<string>();

        private GameContext gameContext;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            InitUIObject();
            mainUIController = mainUIRoot.GetComponent<MainUIController>();

            toggleMenuObject.SetActive(false);
        }

        private void InitUIObject()
        {
            mainUIRoot = GameObject.FindWithTag(MAIN_UI_TAG);
            buttonGroupObject = mainUIRoot.transform.GetChild(0).gameObject;
            toggleMenuObject = mainUIRoot.transform.GetChild(1).gameObject;
            startButton = buttonGroupObject.transform.GetChild(0).GetComponent<Button>();
            mainMenuButton = buttonGroupObject.transform.GetChild(1).GetComponent<Button>();
            exitButton = buttonGroupObject.transform.GetChild(2).GetComponent<Button>();
            nextBattleButton = buttonGroupObject.transform.GetChild(3).GetComponent<Button>();
        }

        private void Update()
        {
            if (SimpleInput.GetKeyDown(Key.Escape))
            {
                if (SceneManager.GetActiveScene().name != MAINMENU_SCENE)
                {
                    bool isToggleMenuActive = toggleMenuObject.activeSelf;
                    toggleMenuObject.SetActive(!isToggleMenuActive);
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
            if (scene.name == BATTLE_SCENE)
            {
                SceneBinder sceneBinder = FindFirstObjectByType<SceneBinder>();
                sceneBinder.BindSceneObjects(gameContext);
            }
        }

        public void HandleBattleResult(EBattleResult eBattleResult)
        {
            // 함선이 파괴되면
            switch (eBattleResult)
            {
                case EBattleResult.PlayerShipDestroyed:
                    Debug.Log("Player ship destroyed");
                    mainUIController.MoveScene(ESceneIndex.GameOver);
                    break;
                case EBattleResult.PlayerAllCrewDead:
                    // 게임오버 화면으로
                    // 어떤 Uicontroller인지 몰라도 얘한테 시켜서 게임오버화면으로 이동
                    mainUIController.MoveScene(ESceneIndex.GameOver);
                    Debug.Log("Player all crew dead.");
                    break;
                case EBattleResult.EnemyShipDestroyed:
                    SetGameContext(Player);
                    gameContext.ScrapAmount += 100;
                    mainUIController.MoveScene(ESceneIndex.BattlePrepare);
                    Debug.Log("Enemy ship destroyed.");
                    break;
                case EBattleResult.EnemyAllCrewDead:
                    SetGameContext(Player);
                    gameContext.ScrapAmount += 200;
                    mainUIController.MoveScene(ESceneIndex.BattlePrepare);
                    Debug.Log("Enemy all crew dead.");
                    break;
                case EBattleResult.PlayerRun:
                    // 전투준비 화면으로 이동
                    // 어떤 Uicontroller인지 몰라도 얘한테 시켜서 전투준비화면으로 이동
                    SetGameContext(Player);
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

        public void SetGameContext(List<Room> rooms, List<FacilitySystem> facilities, List<CrewController> crews, int shipHp, int shipMaxHp, int scrapAmount, int shipReactorCapacity)
        {
            if (gameContext == null)
            {
                gameContext = new GameContext(rooms, facilities, crews, scrapAmount, shipHp, shipMaxHp, shipReactorCapacity);
            }
            else
            {
                gameContext.SetGameContext(rooms, facilities, crews, scrapAmount, shipHp, shipMaxHp, shipReactorCapacity);
            }
        }

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

            gameContext = new GameContext(rooms, facilityStates, crewStates, player.ScrapAmount, player.Hp, player.ShipData.MaxHp, player.PowerSystem.GetReactorMaxPower);
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

        public void IncreaseMaxPowerLimitOfFacility(ERoomType roomType, int amount=1)
        {
            gameContext.IncreaseMaxPowerLimitOfFacility(roomType, amount);
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


        public void AddShipHp(int amount)
        {
            gameContext.AddShipHp(amount);
        }

        #region Test Method
        public void TestHandleBattleResult()
        {
            if (SimpleInput.GetKeyDown(Key.Digit1))
                BattleResult = EBattleResult.PlayerShipDestroyed;
            if (SimpleInput.GetKeyDown(Key.Digit2))
                BattleResult = EBattleResult.PlayerAllCrewDead;
            if (SimpleInput.GetKeyDown(Key.Digit3))
                BattleResult = EBattleResult.PlayerRun;
            if (SimpleInput.GetKeyDown(Key.Digit4))
                BattleResult = EBattleResult.EnemyAllCrewDead;
            if (SimpleInput.GetKeyDown(Key.Digit5))
                BattleResult = EBattleResult.EnemyShipDestroyed;
            if (SimpleInput.AnyKeyDown())
                HandleBattleResult(BattleResult);

        }

        public void TurnOffToggleMenu()
        {
            toggleMenuObject.SetActive(false);
        }

        public void ReduceScrap(int v)
        {
            gameContext.ReduceScrap(v);
        }

        #endregion
    }
}
