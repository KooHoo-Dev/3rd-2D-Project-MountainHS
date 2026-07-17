using System.Collections;
using UnityEngine;

namespace AllIn1SpriteShader
{
    public class Demo2AutoScroll : MonoBehaviour
    {
        public float totalTime;
        public GameObject sceneDescription;
        private Transform[] children;

        private void Start()
        {
            sceneDescription.SetActive(false);
            Camera.main.fieldOfView = 60f;
            children = GetComponentsInChildren<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].gameObject != gameObject)
                {
                    children[i].gameObject.SetActive(false);
                    children[i].localPosition = Vector3.zero;
                }
            }

            totalTime = totalTime / children.Length;

            StartCoroutine(ScrollElements());
        }

        private IEnumerator ScrollElements()
        {
            int i = 0;
            while (true)
            {
                if (children[i].gameObject == gameObject)
                {
                    i = (i + 1) % children.Length;
                    continue;
                }

                children[i].gameObject.SetActive(true);
                yield return new WaitForSeconds(totalTime);
                children[i].gameObject.SetActive(false);
                i = (i + 1) % children.Length;
            }
        }
    }
}
