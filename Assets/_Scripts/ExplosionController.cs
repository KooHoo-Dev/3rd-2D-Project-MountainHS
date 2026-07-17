using UnityEngine;

namespace FTL
{
    public class ExplosionController : MonoBehaviour
    {
        [SerializeField] private float explosionDuration = 0.12f;
        private float elapsed;

        #region Unity Method

        private void OnEnable()
        {
            elapsed = 0f;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed >= explosionDuration)
            {
                gameObject.SetActive(false);
            }
        }

        #endregion
    }
}
