using System.Collections;
using UnityEngine;

namespace FTL
{
    public class FloatingTextController: MonoBehaviour
    {
        // 그냥 나타나면 뿅하고 나타나서 몇초 뒤면 코루틴으로 사라지게
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float speed = 3f;
        [SerializeField] private Vector3 direction = Vector3.up;

        private void Start()
        {
            StartCoroutine(DisableCoroutine(lifetime));
        }

        private void Update()
        {
            transform.Translate(direction * (speed * Time.deltaTime));
        }

        private IEnumerator DisableCoroutine(float lifetime)
        {
            yield return new WaitForSeconds(lifetime);
            gameObject.SetActive(false);
        }
    }
}
