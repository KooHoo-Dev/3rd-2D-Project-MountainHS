using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace FTL
{
    public class ProjectileController : MonoBehaviour
    {
        private const string AREA_DIVIDER_TAG = "AreaDivider";
        [SerializeField] private float damage;
        [SerializeField] private float speed;
        [SerializeField] private Collider2D startCol;
        [SerializeField] private Vector2 target;
        [SerializeField] private LayerMask shipLayer;

        private void Awake()
        {
            startCol = null;
        }

        private void Update()
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
        // SetTarget

        public void SetTarget(Vector2 targetPosition)
        {
            target = targetPosition;
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 충돌한 콜라이더가 함선 레이어 콜라이더이면
            if (startCol == null)
            {
                if (shipLayer.Contains(collision.gameObject.layer))
                {
                    startCol = collision;
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // 특정 콜라이더이면 그에 대응되는 EShipArea가 뽑히도록 하면 좋을 거 같은데
            if (startCol == collision)
            {
                // 전투 시스템 호출
                CombatSystem.Instance.MoveToEnemyArea(startCol, this);
            }
        }
    }
}
