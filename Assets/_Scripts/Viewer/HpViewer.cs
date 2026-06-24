using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

namespace FTL
{
    public enum EHpBarStatus { Available, UnAvailable }
    public class HpViewer : MonoBehaviour
    {
        [SerializeField] private GameObject barPrefab;
        [SerializeField] private GameObject barGroup;
        [SerializeField] private List<UnityEngine.UI.Image> barSprites;
        [SerializeField] private ShipController playerSystem;
        [SerializeField] private int prevHp;
        //protected ShipData powerSystem;
        private ObjectPool<GameObject> barPool;

        private void Awake()
        {
            //powerSystem = systemMono as IPowerInfo;
            barPool = new ObjectPool<GameObject>(CreateBar, OnGetBar, OnReleaseBar, OnDestroyBar);
        }

        private void LateUpdate()
        {
            UpdateViewer();
        }

        // 전력바 상태 관리: 가용, 불가능
        // 전력바 상태에 따라 스프라이트 상태 변경
        private void SetBarState(int barIndex, EHpBarStatus status)
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
            int hp = playerSystem.Hp;
            int maxHp = playerSystem.ShipData.MaxHp;
            
            if (hp == prevHp)
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

        // 반응로 전력 추가
        public void AddBar()
        {
            GameObject powerBar = barPool.Get();
            barSprites.Add(powerBar.GetComponent<UnityEngine.UI.Image>());
            SetBarState(barSprites.Count - 1, EHpBarStatus.Available);
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
