using UnityEngine;

namespace AllIn1SpriteShader
{
    public class DemoItem : MonoBehaviour
    {
        private static readonly Vector3 lookAtZ = new(0, 0, 1);

        private void Update()
        {
            transform.LookAt(transform.position + lookAtZ);
        }
    }
}
