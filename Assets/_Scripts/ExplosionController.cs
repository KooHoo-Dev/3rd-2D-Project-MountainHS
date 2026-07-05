using UnityEngine;

namespace FTL
{
    public class ExplosionController: MonoBehaviour
    {
        private float elapsed = 0f;
        [SerializeField] private float explosionDuration = 0.12f;
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
    }
}
