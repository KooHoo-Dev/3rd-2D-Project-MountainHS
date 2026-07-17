using UnityEngine;
using UnityEngine.SceneManagement;

namespace FTL
{
    public class MainUIController : MonoBehaviour
    {
        [SerializeField] private BattleResultViewer brViewer;
        private bool HasViewer => brViewer != null;

        #region Unity Method

        private void Start()
        {
            if (HasViewer)
            {
                brViewer.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Public Method

        public void MoveScene(int index)
        {
            if (index == 0)
            {
                GameManager.Instance.TurnOffToggleMenu();
            }

            SceneManager.LoadScene(index);
        }

        public void MoveScene(SceneIndex eSceneIndex)
        {
            MoveScene((int)eSceneIndex);
        }

        public void ReturnToMainMenu()
        {
            GameManager.Instance.ResetGameContext();
            MoveScene(SceneIndex.MainMenu);
        }

        public void ExitGame()
        {
            GameManager.Instance.ExitGame();
        }

        public void HandlePlayerRun()
        {
            GameManager.Instance.HandleBattleResult(BattleResult.PlayerRun);
        }

        public void SetBattleResult(BattleResult battleResult, int reward)
        {
            if (HasViewer)
            {
                brViewer.SetBattleResult(battleResult, reward);
                brViewer.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning(
                    "BattleResultViewer is not assigned in MainUIController. Cannot display battle result.");
            }
        }

        #endregion
    }
}
