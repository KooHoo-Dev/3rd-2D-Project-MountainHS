using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace FTL
{
    public class PowerViewer : MonoBehaviour
    {
        [SerializeField] protected GameObject barPrefab;
        [SerializeField] protected GameObject barGroup;
        [SerializeField] protected List<Image> barSprites = new();
        [SerializeField] protected FacilityController facilitySystem;
        protected ObjectPool<GameObject> barPool;

        #region Unity Method

        protected virtual void Awake()
        {
            barPool = new ObjectPool<GameObject>(CreateBar, OnGetBar, OnReleaseBar, OnDestroyBar);
        }

        protected void LateUpdate()
        {
            UpdateViewer();
        }

        #endregion

        #region Public Method

        // 전력바 상태 관리: 가용, 불가능
        // 전력바 상태에 따라 스프라이트 상태 변경
        protected virtual void SetBarState(int barIndex, PowerBarStatus status)
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
                case PowerBarStatus.Available:
                    image.color = Color.green; // 가용 상태 색상
                    break;
                case PowerBarStatus.Broken:
                    image.color = Color.orange; // 불가능 상태 색상
                    break;
                case PowerBarStatus.UnAvailable:
                    image.color = Color.grey; // 불가능 상태 색상
                    break;
                default:
                    Debug.LogError($"Unknown power bar status: {status}");
                    break;
            }
        }

        // 현재 전력 상태 (총 용량, 가용 가능한 양 상태에 따라 power bar 업데이트)
        protected virtual void UpdateViewer()
        {
            int totalPower = facilitySystem.MaxPower;
            int availablePower = facilitySystem.Power;
            // 만약 현재 powerBarSprites 수가 실제 totalPower 와 일치하지 않는 다면 생성
            if (barSprites.Count < totalPower)
            {
                int n = totalPower - barSprites.Count;
                for (int i = 0; i < n; i++)
                {
                    AddBar();
                }
            }

            for (int i = 0; i < barSprites.Count; i++)
            {
                if (i < availablePower)
                {
                    SetBarState(i, PowerBarStatus.Available);
                }
                else
                {
                    SetBarState(i, PowerBarStatus.UnAvailable);
                }
            }
        }

        // 반응로 전력 추가
        public void AddBar()
        {
            GameObject powerBar = barPool.Get();
            barSprites.Add(powerBar.GetComponent<Image>());
            SetBarState(barSprites.Count - 1, PowerBarStatus.Available);
        }

        #endregion

        #region Private Method

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
