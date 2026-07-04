using System;
using System.Collections;
using UnityEngine;
using Utilities;

namespace FTL
{
    [Serializable]
    public struct ProjectileTargetData
    {
        public Vector3 TargetPosition;
        public int RoomId;

        public ProjectileTargetData(Vector3 targetPosition, int roomId)
        {
            TargetPosition = targetPosition;
            RoomId = roomId;
        }

        public void Reset()
        {
            TargetPosition = Vector3.zero;
            RoomId = -1;
        }

        public bool IsValid()
        {
            return TargetPosition != Vector3.zero && RoomId >= 0;
        }
    }

    [RequireComponent(typeof(SpriteRenderer))]
    public class ProjectileController : MonoBehaviour
    {
        public bool IsArrived => transform.position == TargetPos;
        public int TargetRoomId => targetRoomId;
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField] public int ShieldDamage { get; private set; }
        [field: SerializeField] public int CrewDamage { get; private set; }
        [SerializeField] private float speed;
        [field: SerializeField] public Collider2D StartCol { get; private set; }
        [SerializeField] private Collider2D endCol;
        [field: SerializeField] public Vector3 TargetPos { get; private set; } = Vector3.zero;
        [field: SerializeField] public float RandomSeed { get; private set; } = 0f;

        [SerializeField] private LayerMask shipLayer;
        [SerializeField] private LayerMask shieldLayer;
        [SerializeField] private int targetRoomId;
        [SerializeField] private bool isAfterStartCol = false;
        [SerializeField] private bool isMoving = true;

        private SpriteRenderer renderer;

        private void Awake()
        {
            StartCol = null;
            endCol = null;
            targetRoomId = -1;
            isAfterStartCol = false;
            renderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // 목적지에 도착했으면 삭제
            if (IsArrived)
            {
                // 전투 시스템에 판정 요청
                // 회피이면 무시
                // 명중이면 삭제
                CombatSystem.Instance.HandleRoomCollision(this);
                Destroy(gameObject);
            }

            if (isMoving)
            {
                if (isAfterStartCol == false)
                {
                    Vector3 moveAmount = Vector3.right * speed * Time.deltaTime;
                    transform.Translate(moveAmount);
                }
                else
                {
                    float moveDistance = speed * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, TargetPos, moveDistance);
                }
            }
        }
        // SetTarget

        public void SetTarget(ProjectileTargetData targetData)
        {
            TargetPos = targetData.TargetPosition;
            targetRoomId = targetData.RoomId;
        }

        public void SetEndCol(Collider2D endCol)
        {
            this.endCol = endCol;
        }

        public void SetRandomSeed(float seed)
        {
            RandomSeed = seed;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 충돌한 콜라이더가 함선 레이어 콜라이더이면
            bool hasNoStartCol = StartCol == null;
            if (hasNoStartCol)
            {
                bool isShipLayer = shipLayer.Contains(collision.gameObject.layer);
                if (isShipLayer)
                {
                    StartCol = collision;
                }
            }

            // 충돌한 콜라이더가 실드 레이어 && 태그가 enemyTag라면
            bool isShieldLayer = shieldLayer.Contains(collision.gameObject.layer);
            bool canHitShield = isAfterStartCol && isShieldLayer;
            if (canHitShield)
            {
                // 전투 시스템 호출
                bool isShielded = CombatSystem.Instance.HandleShieldCollision(this);
                if (isShielded)
                {
                    Destroy(gameObject);
                }
            }

        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // 특정 콜라이더이면 그에 대응되는 EShipArea가 뽑히도록 하면 좋을 거 같은데
            bool isLeavingStartCol = StartCol == collision;
            if (isLeavingStartCol)
            {
                // 전투 시스템 호출
                CombatSystem.Instance.MoveToEnemyArea(StartCol, this);
                isAfterStartCol = true;
                isMoving = false;
                StartCoroutine(SetProjectileDelay());
            }
            // 종료 콜라이더 벗어나면 삭제
            bool isLeavingEndCol = endCol == collision;
            if (isLeavingEndCol)
            {
                // 전투 시스템 호출
                Destroy(gameObject);
            }
        }

        private IEnumerator SetProjectileDelay()
        {
            const float MAX_SPEED = 11f;
            const float MIN_SPEED = 3f;
            const float MIN_TIME = 0.5f;
            const float MAX_TIME = 2f;
            // 진행도

            // 속도 11 ~ 3
            float lerpRate = Mathf.InverseLerp(MAX_SPEED, MIN_SPEED, speed);
            // 목표 시간: 0.5 ~ 2
            float delayTime = Mathf.Lerp(MIN_TIME, MAX_TIME, lerpRate);
            renderer.enabled = false;

            yield return new WaitForSeconds(delayTime);
            renderer.enabled = true;
            isMoving = true;
        }
    }
}
