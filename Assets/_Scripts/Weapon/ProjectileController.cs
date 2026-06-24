using UnityEngine;

namespace FTL
{
    public class ProjectileController : MonoBehaviour
    {
        [SerializeField] private float damage;
        [SerializeField] private float speed;
        [SerializeField] private Vector2 target;

        // SetTarget
        public void SetTarget(Vector2 targetPosition)
        {
            target = targetPosition;
        }

        // 플레이어 화면 내에서 움직임
        // 적 화면 내에서 움직임
    }
}
