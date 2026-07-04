using System.Collections;
using TMPro;
using UnityEngine;

namespace FTL
{
    [RequireComponent(typeof(TMP_Text))]
    public class FloatingTextController: MonoBehaviour
    {
        // 그냥 나타나면 뿅하고 나타나서 몇초 뒤면 코루틴으로 사라지게
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float speed = 3f;
        [SerializeField] private float xRange = 1f;
        [SerializeField] private float yOffset = 0.5f;
        [SerializeField] private Vector3 direction = Vector3.up;
        [SerializeField] private TMP_Text text;

        private void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            float xOffset = Random.Range(-xRange, xRange); // -1, 0, 1 중 하나를 랜덤으로 선택
            Vector3 newPosition = transform.position;
            newPosition.x += xOffset;
            newPosition.y += yOffset;
            transform.position = newPosition;
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

        public void SetText(string value)
        {
            text.SetText(value);
        }

        public void SetTextColor(Color color)
        {
            text.color = color;
        }
    }
}
