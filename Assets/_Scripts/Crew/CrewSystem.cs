using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace FTL
{
    public class CrewSystem: MonoBehaviour
    {
        public List<Room> Rooms { get; private set; }
        [field: SerializeField] public ShipController ship;
        private List<FacilitySystem> facilitySystems = new List<FacilitySystem>();
        [SerializeField] private bool isPlayer = true;
        [SerializeField] private List<CrewController> crews;

        private void Update()
        {
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
    }
}
