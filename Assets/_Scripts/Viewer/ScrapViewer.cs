using TMPro;
using UnityEngine;

namespace FTL
{
    public class ScrapViewer: MonoBehaviour
    {
        TMP_Text text;

        #region Unity Method

        private void Awake()
        {
            text = transform.GetChild(2).GetComponent<TMP_Text>();
        }

        private void LateUpdate()
        {
            int currentScrap = GameManager.Instance.GetScrap();
            text.SetText($"{currentScrap}");
        }

        #endregion
    }
}
