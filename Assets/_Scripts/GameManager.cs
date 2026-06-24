using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Utilities;

namespace FTL
{
    /// <summary>
    /// GameManager는 전투 종료, 게임 종료 같은 씬간 정보를 관리
    /// 싱글톤 써야함
    /// </summary>
    public class GameManager : SingletonBase<GameManager>
    {
        /// <summary>
        /// 이것도 이게 맞나...
        /// 게임 종료 상태를 읽어오는게 아니라
        /// 특정 조건이면 그냥 다음 화면으로 가는게 맞는거 같은데
        /// 아 그냥 조건문에 넣을 함수로 쓰는건 ㅇㅈ
        /// </summary>
        /// <returns></returns>
        [SerializeField] private MainUIController mainUIController;
        [SerializeField] private EBattleResult battleResult;

        private void Update()
        {
            //TestHandleBattleResult();
        }
        
        public bool HandleBattleResult(EBattleResult eBattleResult)
        {
            // 함선이 파괴되면
            switch (eBattleResult)
            {
                case EBattleResult.PlayerShipDestroyed:
                case EBattleResult.PlayerAllCrewDead:
                    // 게임오버 화면으로
                    // 어떤 Uicontroller인지 몰라도 얘한테 시켜서 게임오버화면으로 이동
                    mainUIController.MoveScene(ESceneIndex.GameOver);
                    break;
                case EBattleResult.EnemyShipDestroyed:
                case EBattleResult.EnemyAllCrewDead:
                case EBattleResult.PlayerRun:
                    // 전투준비 화면으로 이동
                    // 어떤 Uicontroller인지 몰라도 얘한테 시켜서 전투준비화면으로 이동
                    mainUIController.MoveScene(ESceneIndex.BattlePrepare);
                    break;
                case EBattleResult.None:
                default:
                    Debug.LogError("Unknown EBattleResult");
                    break;
            }
            // 임시
            return false;
        }

        public void TestHandleBattleResult()
        {
            if (SimpleInput.GetKeyDown(Key.Digit1))
                battleResult = EBattleResult.PlayerShipDestroyed;
            if (SimpleInput.GetKeyDown(Key.Digit2))
                battleResult = EBattleResult.PlayerAllCrewDead;
            if (SimpleInput.GetKeyDown(Key.Digit3))
                battleResult = EBattleResult.PlayerRun;
            if (SimpleInput.GetKeyDown(Key.Digit4))
                battleResult = EBattleResult.EnemyAllCrewDead;
            if (SimpleInput.GetKeyDown(Key.Digit5))
                battleResult = EBattleResult.EnemyShipDestroyed;
            if (SimpleInput.AnyKeyDown())
                HandleBattleResult(battleResult);

        }
    }
}
