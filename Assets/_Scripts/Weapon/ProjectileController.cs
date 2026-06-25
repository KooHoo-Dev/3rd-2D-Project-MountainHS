using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

namespace FTL
{
    [Serializable]
    public struct SProjectileTargetData
    {
        public Vector3 TargetPosition;
        public int RoomId;

        public SProjectileTargetData(Vector3 targetPosition, int roomId)
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

    public class ProjectileController : MonoBehaviour
    {
        private const string AREA_DIVIDER_TAG = "AreaDivider";

        public bool IsArrived => transform.position == target;
        public int TargetRoomId => targetRoomId;
        [SerializeField] private float damage;
        [SerializeField] private float speed;
        [field: SerializeField] public Collider2D StartCol { get; private set; }
        [SerializeField] private Collider2D endCol;
        [SerializeField] private Vector3 target;
        [SerializeField] private int targetRoomId;
        [SerializeField] private LayerMask shipLayer;
        [SerializeField] private LayerMask shieldLayer;
        [SerializeField] private bool isAfterStartCol = false;

        private void Awake()
        {
            StartCol = null;
            endCol = null;
            targetRoomId = -1;
            isAfterStartCol = false;
        }

        private void Update()
        {
            // 목적지에 도착했으면 삭제
            if (IsArrived)
            {
                // 전투 시스템에 판정 요청
                // 회피이면 무시
                // 명중이면 삭제
                Destroy(gameObject);
            }

            if (isAfterStartCol == false)
                transform.Translate(Vector3.right * speed * Time.deltaTime);
            else
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        }
        // SetTarget

        public void SetTarget(SProjectileTargetData targetData)
        {
            target = targetData.TargetPosition;
            targetRoomId = targetData.RoomId;
        }

        public void SetEndCol(Collider2D endCol)
        {
            this.endCol = endCol;
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 충돌한 콜라이더가 함선 레이어 콜라이더이면
            if (StartCol == null)
            {
                if (shipLayer.Contains(collision.gameObject.layer))
                {
                    StartCol = collision;
                }
            }

            // 충돌한 콜라이더가 실드 레이어 && 태그가 enemyTag라면
            if (isAfterStartCol && shieldLayer.Contains(collision.gameObject.layer))
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
            if (StartCol == collision)
            {
                // 전투 시스템 호출
                CombatSystem.Instance.MoveToEnemyArea(StartCol, this);
                isAfterStartCol = true;
            }
            // 종료 콜라이더 벗어나면 삭제
            if (endCol == collision)
            {
                // 전투 시스템 호출
                Destroy(gameObject);
            }
        }
    }
}
