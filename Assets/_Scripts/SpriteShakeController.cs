using System.Collections;
using UnityEngine;

namespace FTL
{
    public class SpriteShakeController: MonoBehaviour
    {
        [SerializeField] private float xRange = 0.1f;
        [SerializeField] private float yRange = 0.1f;
        [SerializeField] private float shakeStrengthOffset = 0.02f;
        [field: SerializeField] public bool IsShaked { get; private set; } = false;
        [SerializeField] private Transform spriteParentTransform;
        [SerializeField] private SpriteRenderer spriteRenderer;

        #region Unity Method

        private void Awake()
        {
            IsShaked = false;
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            if (spriteParentTransform == null)
            {
                spriteParentTransform = spriteRenderer.transform;
            }
        }

        #endregion

        #region Public Method

        public void ShakeSprite()
        {
            StartCoroutine(MoveSpriteObjectToRandomOffset());
        }

        #endregion

        #region Private Method

        private IEnumerator MoveSpriteObjectToRandomOffset()
        {
            if (IsShaked)
            {
                xRange += shakeStrengthOffset;
                yRange += shakeStrengthOffset;
                yield break;
            }

            IsShaked = true;
            Vector3 startPosition = spriteParentTransform.position;
            float startXRange = xRange;
            float startYRange = yRange;
            for (int i = 0; i < 10; i++)
            {
                Vector3 newPosition = startPosition;
                newPosition += GetRandomOffset(xRange, yRange);
                spriteParentTransform.position = newPosition;
                yield return new WaitForSeconds(0.05f);
            }

            spriteParentTransform.position = startPosition;
            IsShaked = false;
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
