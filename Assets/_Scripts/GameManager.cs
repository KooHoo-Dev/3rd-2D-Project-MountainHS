using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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

        public bool IsGameContextEmpty => gameContext == null;
        public PlayerController Player;


        [SerializeField] private MainUIController mainUIController;
        [field: SerializeField] public EBattleResult BattleResult { get; set; }
        private GameContext gameContext;
        
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
            if (scene.name != BATTLE_SCENE)
            {
                return;
            }

            SceneBinder sceneBinder = FindFirstObjectByType<SceneBinder>();
            sceneBinder.BindSceneObjects(gameContext);
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

        public void SetGameContext(List<Room> rooms, List<FacilitySystem> facilities, List<CrewController> crews, int shipHp, int scrapAmount, int shipReactorCapacity)
        {
            if (gameContext == null)
            {
                gameContext = new GameContext(rooms, facilities, crews, scrapAmount, shipHp, shipReactorCapacity);
            }
            else
            {
                gameContext.SetGameContext(rooms, facilities, crews, scrapAmount, shipHp, shipReactorCapacity);
            }
        }

        public void SetGameContext(PlayerController player)
        {
            List<Room> rooms = player.Rooms;
            List<FacilitySystem> facilities = player.FacilitySystems;
            List<CrewController> crews = player.Crews;
            if (gameContext == null)
            {
                gameContext = new GameContext(rooms, facilities, crews, player.ScrapAmount, player.Hp, player.PowerSystem.GetReactorMaxPower);
            }
            else
            {
                gameContext.SetGameContext(rooms, facilities, crews, player.ScrapAmount, player.Hp, player.PowerSystem.GetReactorMaxPower);
            }
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

        #region
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
        #endregion
    }
}
