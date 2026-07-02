using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FTL
{
    public class MainUIController : MonoBehaviour
    {
        private bool HasViewer => brViewer != null;
        [SerializeField] private BattleResultViewer brViewer;

        private void Start()
        {
            if (HasViewer)
            {
                brViewer.gameObject.SetActive(false);
            }
        }

        public void MoveScene(int index)
        {
            if (index == 0)
            {
                GameManager.Instance.TurnOffToggleMenu();
            }

            SceneManager.LoadScene(index);
        }

        public void MoveScene(ESceneIndex eSceneIndex)
        {
            MoveScene((int)eSceneIndex);
        }

        public void ReturnToMainMenu()
        {
            GameManager.Instance.ResetGameContext();
            MoveScene(ESceneIndex.MainMenu);
        }

        public void ExitGame()
        {
            GameManager.Instance.ExitGame();
        }

        public void HandlePlayerRun()
        {
            GameManager.Instance.HandleBattleResult(EBattleResult.PlayerRun);
        }

        public void SetBattleResult(EBattleResult battleResult, int reward)
        {
            if (HasViewer)
            {
                brViewer.SetBattleResult(battleResult, reward);
                brViewer.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("BattleResultViewer is not assigned in MainUIController. Cannot display battle result.");
            }
        }
    }
}
