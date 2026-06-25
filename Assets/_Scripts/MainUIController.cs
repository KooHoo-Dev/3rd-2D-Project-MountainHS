using UnityEngine;
using UnityEngine.SceneManagement;

namespace FTL
{
    public class MainUIController : MonoBehaviour
    {
        private void MoveScene(int index)
        {
            SceneManager.LoadScene(index);
        }
        public void MoveScene(ESceneIndex eSceneIndex)
        {
            MoveScene((int)eSceneIndex);
            //SceneManager.LoadScene((int)eSceneIndex);
        }
    }
}
