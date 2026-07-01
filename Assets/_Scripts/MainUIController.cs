using UnityEngine;
using UnityEngine.SceneManagement;

namespace FTL
{
    public class MainUIController : MonoBehaviour
    {
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
    }
}
