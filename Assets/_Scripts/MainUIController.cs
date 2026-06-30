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

        public void ExitGame()
        {
            GameManager.Instance.ExitGame();
        }
    }
}
