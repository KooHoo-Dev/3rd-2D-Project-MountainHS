using TMPro;
using UnityEngine;

namespace FTL
{
    public class BattleResultViewer : MonoBehaviour
    {
        [SerializeField] private TMP_Text battleResultText;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private BattleResult battleResult = BattleResult.None;

        #region Public Method

        // 나중에 보상 종류 다양해지면 클래스나 인스턴스로 묶어야겠네
        public void SetBattleResult(BattleResult battleResult, int reward)
        {
            bool isWin = battleResult == BattleResult.EnemyAllCrewDead ||
                         battleResult == BattleResult.EnemyShipDestroyed;
            string battleResultString = battleResult.GetBattleResultName();
            string rewardString = isWin ? $"Scrap   +{reward}" : "";
            battleResultText.SetText(battleResultString);
            rewardText.SetText(rewardString);
            this.battleResult = battleResult;
        }

        public void DisableBattleResultViewer()
        {
            gameObject.SetActive(false);
            GameManager.Instance.MoveSceneByBattleResult(battleResult);
        }

        #endregion
    }
}
