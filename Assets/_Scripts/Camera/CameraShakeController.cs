
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace FTL
{
    public class CameraShakeController : MonoBehaviour
    {
        [SerializeField] private float xRange;
        [SerializeField] private float yRange;
        [SerializeField] private float shakeStrengthOffset = 1f;
        [SerializeField] private bool isShaked = false;

        #region Unity Method

        private void Awake()
        {
            isShaked = false;
        }

        #endregion

        #region Public Method

        public void ShakeCamera()
        {
            StartCoroutine(MoveCameraToRandomRange());
        }

        #endregion

        #region Private Method

        private IEnumerator MoveCameraToRandomRange()
        {
            if (isShaked)
            {
                xRange += shakeStrengthOffset;
                yRange += shakeStrengthOffset;
                yield break;
            }

            isShaked = true;
            Vector3 cameraStartPos = Camera.main.transform.position;
            float startXRange = xRange;
            float startYRange = yRange;
            for (int i = 0; i < 10; i++)
            {
                Vector3 newCameraPos = cameraStartPos;
                newCameraPos += GetRandomOffset(xRange, yRange);
                Camera.main.transform.position = newCameraPos;
                yield return new WaitForSeconds(0.05f);
            }

            Camera.main.transform.position = cameraStartPos;
            isShaked = false;
            xRange = startXRange;
            yRange = startYRange;
        }

        private Vector3 GetRandomOffset(float xRange, float yRange)
        {
            float xOffset = Random.Range(-xRange, xRange);
            float yOffset = Random.Range(-yRange, yRange);
            return new Vector3(xOffset, yOffset, 0f);
        }

        #endregion
    }
}
