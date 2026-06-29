using TMPro;
using UnityEngine;

namespace FTL
{
    public class ScrapViewer: MonoBehaviour
    {
        TMP_Text text;

        private void Awake()
        {
            text = GetComponentInChildren<TMP_Text>();
        }

        private void LateUpdate()
        {
            text.SetText($"{GameManager.Instance.GetScrap()}");
        }
    }
}
