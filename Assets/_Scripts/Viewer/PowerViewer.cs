using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace FTL
{
    public enum EPowerBarStatus { Available, Broken, UnAvailable, }
    public class PowerViewer : MonoBehaviour
    {
        [SerializeField] protected GameObject barPrefab;
        [SerializeField] protected GameObject barGroup;
        [SerializeField] protected List<UnityEngine.UI.Image> barSprites;
        [SerializeField] protected MonoBehaviour powerSystemMono;
        protected IPowerInfo powerSystem;
        protected ObjectPool<GameObject> barPool;

        protected virtual void Awake()
        {
            powerSystem = powerSystemMono as IPowerInfo;
            barPool = new ObjectPool<GameObject>(CreateBar, OnGetBar, OnReleaseBar, OnDestroyBar);
        }

        protected void LateUpdate()
        {
            UpdateViewer();
        }

        // 전력바 상태 관리: 가용, 불가능
        // 전력바 상태에 따라 스프라이트 상태 변경
        protected virtual void SetBarState(int barIndex, EPowerBarStatus status)
        {
            if (barIndex < 0 || barIndex >= barSprites.Count)
            {
                Debug.LogWarning($"Invalid bar index: {barIndex}");
                return;
            }

            UnityEngine.UI.Image image = barSprites[barIndex];
            // 스프라이트 상태 변경
            switch (status)
            {
                case EPowerBarStatus.Available:
                    image.color = Color.green; // 가용 상태 색상
                    break;
                case EPowerBarStatus.Broken:
                    image.color = Color.orange; // 불가능 상태 색상
                    break;
                case EPowerBarStatus.UnAvailable:
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
            int totalPower = powerSystem.MaxPower;
            int availablePower = powerSystem.Power;
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
                    SetBarState(i, EPowerBarStatus.Available);
                }
                else
                {
                    SetBarState(i, EPowerBarStatus.UnAvailable);
                }
            }
        }

        // 반응로 전력 추가
        public void AddBar()
        {
            GameObject powerBar = barPool.Get();
            barSprites.Add(powerBar.GetComponent<UnityEngine.UI.Image>());
            SetBarState(barSprites.Count - 1, EPowerBarStatus.Available);
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
    }
}
