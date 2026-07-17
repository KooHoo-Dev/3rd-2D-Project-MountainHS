using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class SolarFlareController : MonoBehaviour
    {
        [SerializeField] private Image flareImage;
        [field: SerializeField] public float FlareDuration { get; private set; } = 0.5f;
        [SerializeField] private Color startColor;
        [SerializeField] private Color targetColor;

        #region Unity Method

        private void Start()
        {
            flareImage.color = startColor;
        }

        #endregion

        #region Public Method

        [ContextMenu("Flare Test")]
        public void ApplyFlareEffect()
        {
            StartCoroutine(TurnOnFlare(FlareDuration));
        }

        #endregion

        #region Private Method

        private IEnumerator TurnOnFlare(float flareDuration)
        {
            float elapsed = 0f;
            while (true)
            {
                if (elapsed >= flareDuration)
                {
                    break;
                }

                Color newColor = Color.Lerp(startColor, targetColor, elapsed / flareDuration);
                flareImage.color = newColor;
                elapsed += Time.deltaTime;
                yield return null;
            }

            StartCoroutine(TurnOffFlare(flareDuration));
        }

        private IEnumerator TurnOffFlare(float flareDuration)
        {
            float elapsed = 0f;
            while (true)
            {
                if (elapsed >= flareDuration)
                {
                    break;
                }

                Color newColor = Color.Lerp(targetColor, startColor, elapsed / flareDuration);
                flareImage.color = newColor;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        #endregion
    }
}
