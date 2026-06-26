using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace FTL
{
    public class CrewSystem: MonoBehaviour
    {

        private static readonly int GLOW_EFFECT = Shader.PropertyToID("_Glow");

        public List<Room> Rooms { get; private set; }
        [field: SerializeField] private ShipController Ship;
        private List<FacilitySystem> facilitySystems = new List<FacilitySystem>();
        [SerializeField] private List<CrewController> crews = new List<CrewController>();
        [SerializeField] private List<CrewController> selectedCrews = new List<CrewController>();
        [SerializeField] private bool isPlayer = true;
        MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
            CrewController[] crews = GetComponentsInChildren<CrewController>();
            for (int i = 0; i < crews.Length; i++)
            {
                this.crews.Add(crews[i]);
            }
        }
        private void Start()
        {
            facilitySystems.Add(GetComponent<WeaponSystem>());
            facilitySystems.Add(GetComponent<EngineSystem>());
            facilitySystems.Add(GetComponent<ShieldSystem>());

        }

        public void AddSelectedCrew(CrewController crew)
        {
            SpriteRenderer renderer = crew.GetComponent<SpriteRenderer>();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(GLOW_EFFECT, 100f);
            renderer.SetPropertyBlock(propertyBlock);
            selectedCrews.Add(crew);
        }

        public void ResetSelectedCrews()
        {
            int length = selectedCrews.Count;
            for (int i = 0; i < length; i++)
            {
                // 매터리얼 효과 해제
                CrewController crew = selectedCrews[i];
                SpriteRenderer renderer = crew.GetComponent<SpriteRenderer>();
                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(GLOW_EFFECT, 0f);
                renderer.SetPropertyBlock(propertyBlock);
                // 선택한 크루 삭제
                selectedCrews.Remove(crew);
            }
        }
    }
}
