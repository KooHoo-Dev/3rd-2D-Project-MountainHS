using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FTL
{
    public class ShieldViewer : MonoBehaviour
    {
        [SerializeField] private Image[] shieldSquares;
        [SerializeField] private Image shieldChargeBar;
        [SerializeField] private SpriteRenderer shieldMainSprite;
        [SerializeField] private ShieldSystem shieldSystem;
        [SerializeField] private Collider2D col;
        [SerializeField] private Color color;
        [SerializeField] private Vector3 startLocalScale;
        [SerializeField] private float shieldUpDelay;
        [SerializeField] private int prevShieldCount;
        [SerializeField] private bool isEnemy = false;

        private void Awake()
        {
            if (isEnemy == false)
            {
                startLocalScale = shieldMainSprite.transform.localScale;
                col = shieldMainSprite.GetComponent<Collider2D>();
            }
        }

        private void Start()
        {
            if (isEnemy)
            {
                // EnemyShield
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("EnemyShip");

                foreach (GameObject enemy in enemies)
                {
                    if (enemy.layer == LayerMask.NameToLayer("Shield"))
                    {
                        // 찾은 오브젝트
                        shieldMainSprite = enemy.GetComponent<SpriteRenderer>();
                        startLocalScale = shieldMainSprite.transform.localScale;
                        col = shieldMainSprite.GetComponent<Collider2D>();

                    }
                }

                foreach (GameObject enemy in enemies)
                {
                    if (enemy.TryGetComponent<ShieldSystem>(out shieldSystem))
                    {
                        Debug.Log("ShieldViewer: Enemy ShieldSystem Found!");
                        break;
                    }

                }

                if (shieldSystem == null)
                    Debug.LogError("ShieldViewer: There is no shieldMainSprite on Enemy!");
            }
        }
        // 실드 시스템의 최대 실드 개수, 실드 개수에 따라 실드 ui 표시
        private void LateUpdate()
        {
            UpdateViewer();
        }

        // 충전한 정도에 따라서 투명도를 다르게 표시하고 싶은데
        private void UpdateViewer()
        {
            int maxShield = shieldSystem.MaxShield;
            int shieldCount = shieldSystem.ShieldCount;
            int maxShieldLimit = shieldSystem.MaxShieldLimit;

            //if (shieldCount == maxShield
            //    && prevShieldCount == shieldCount)
            //    return;

            UpdateShieldSquares(maxShield, shieldCount, maxShieldLimit);

            UpdateMainShield(shieldCount, maxShieldLimit);

            UpdateChargeBar();

            void UpdateChargeBar()
            {
                float chargeProgress = shieldSystem.ChargeProgress;
                float chargeDuration = shieldSystem.ChargeDuration;
                float chargeRate = chargeProgress / chargeDuration;
                shieldChargeBar.fillAmount = chargeRate;
            }

            void UpdateShieldSquares(int maxShield, int shieldCount, int maxShieldLimit)
            {
                for (int i = 0; i < maxShieldLimit; i++)
                {
                    Color newUIColor = color;
                    if (i < shieldCount)
                        newUIColor.a = 1f;
                    else if (i < maxShield)
                        newUIColor.a = 0.5f;
                    else
                        newUIColor.a = 0f;

                    shieldSquares[i].color = newUIColor;
                }
            }

            void UpdateMainShield(int shieldCount, int maxShieldLimit)
            {
                if (prevShieldCount == 0 && shieldCount == 1)
                {
                    // 스케일 값 0으로 설정
                    shieldMainSprite.transform.localScale = Vector2.zero;
                    // 0.25초동안 스케일 커지기 (1로 커지기)
                    StartCoroutine(ShieldUpCoroutine());
                }
                Color newMainColor = Color.white;
                newMainColor.a = (float)shieldCount / maxShieldLimit;
                shieldMainSprite.color = newMainColor;
                prevShieldCount = shieldCount;
            }
        }

        private IEnumerator ShieldUpCoroutine()
        {
            float inverseShieldUpDelay = 1 / shieldUpDelay;
            Vector3 deltaScale = new Vector3(startLocalScale.x, startLocalScale.y, 0) * inverseShieldUpDelay * Time.deltaTime;
            Transform shieldMainSpriteTransform = shieldMainSprite.transform;
            while (true)
            {
                Vector3 localScale = shieldMainSpriteTransform.localScale;
                if (localScale.x >= startLocalScale.x && localScale.y >= startLocalScale.y)
                    yield break;

                shieldMainSpriteTransform.localScale += deltaScale;
                yield return null;
            }
        }
    }
}
