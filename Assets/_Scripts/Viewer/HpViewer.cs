using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace FTL
{
    public enum EHpBarStatus { Available, UnAvailable }
    public class HpViewer : MonoBehaviour
    {
        [SerializeField] private GameObject barPrefab;
        [SerializeField] private GameObject barGroup;
        [SerializeField] private List<Image> barSprites = new List<Image>();
        [SerializeField] private ShipController ship;
        [SerializeField] private int prevHp;
        [SerializeField] private bool isEnemy = false;
        private ObjectPool<GameObject> barPool;

        #region Unity Method

        private void Awake()
        {
            prevHp = -1;
            barPool = new ObjectPool<GameObject>(CreateBar, OnGetBar, OnReleaseBar, OnDestroyBar);
        }

        private void Start()
        {
            if (isEnemy)
            {
                // EnemyShield
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("EnemyShip");

                foreach (GameObject enemy in enemies)
                {
                    if (enemy.TryGetComponent<ShipController>(out ship))
                    {
                        Debug.Log("HpViewer: Enemy shipController Found!");
                        break;
                    }
                }

                if (ship == null)
                    Debug.LogError("HpViewer: There is no shipController on Enemy!");
            }
        }

        private void LateUpdate()
        {
            UpdateViewer();
        }

        #endregion

        #region Public Method

        // 반응로 전력 추가
        public void AddBar()
        {
            GameObject powerBar = barPool.Get();
            barSprites.Add(powerBar.GetComponent<Image>());
            SetBarState(barSprites.Count - 1, EHpBarStatus.Available);
        }

        #endregion

        #region Private Method

        // 전력바 상태 관리: 가용, 불가능
        // 전력바 상태에 따라 스프라이트 상태 변경
        private void SetBarState(int barIndex, EHpBarStatus status)
        {
            if (barIndex < 0 || barIndex >= barSprites.Count)
            {
                Debug.LogWarning($"Invalid bar index: {barIndex}");
                return;
            }

            Image image = barSprites[barIndex];
            // 스프라이트 상태 변경
            switch (status)
            {
                case EHpBarStatus.Available:
                    image.color = Color.green; // 가용 상태 색상
                    break;
                case EHpBarStatus.UnAvailable:
                    image.color = Color.black; // 불가능 상태 색상
                    break;
            }
        }

        // 현재 전력 상태 (총 용량, 가용 가능한 양 상태에 따라 power bar 업데이트)
        private void UpdateViewer()
        {
            int hp = 0;
            int maxHp = 0;
            if (ship != null)
            {
                hp = ship.Hp;
                maxHp = ship.ShipData.MaxHp;
            }
            else if (TryGetGameContextHp(out hp, out maxHp) == false)
            {
                return;
            }
            
            if (hp == prevHp && barSprites.Count >= maxHp)
            {
                return;
            }

            if (barSprites.Count < maxHp)
            {
                int n = maxHp - barSprites.Count ;
                for (int i = 0; i < n; i++)
                {
                    AddBar();
                }
            }
            for (int i = 0; i < barSprites.Count; i++)
            {
                if (i < hp)
                {
                    SetBarState(i, EHpBarStatus.Available);
                }
                else
                {
                    SetBarState(i, EHpBarStatus.UnAvailable);
                }
            }

            prevHp = hp;
        }

        private bool TryGetGameContextHp(out int hp, out int maxHp)
        {
            hp = 0;
            maxHp = 0;

            if (GameManager.IsInitialized == false)
            {
                Debug.LogError($"{gameObject.name}: GameManager is not Initialized to use!");
                return false;
            }

            GameManager gameManager = GameManager.Instance;
            if (gameManager.IsGameContextEmpty)
            {
                Debug.LogError($"{gameObject.name}: GameManager's GameContext is empty to use!");
                return false;
            }

            hp = gameManager.GetShipHp();
            maxHp = gameManager.GetShipMaxHp();
            return true;
        }

        private GameObject CreateBar()
        {
            return Instantiate(barPrefab, barGroup.transform);
        }

        private void OnGetBar(GameObject powerBar)
        {
            powerBar.transform.SetParent(barGroup.transform);
            powerBar.SetActive(true);
        }

        private void OnReleaseBar(GameObject powerBar)
        {
            powerBar.SetActive(false);
        }

        private void OnDestroyBar(GameObject powerBar)
        {
            Destroy(powerBar);
        }

        #endregion
    }
}
