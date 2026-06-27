using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace FTL
{
    public class CrewSystem: MonoBehaviour
    {

        public List<Room> Rooms { get; private set; }
        [field: SerializeField] private ShipController ship;
        private List<FacilitySystem> facilitySystems = new List<FacilitySystem>();
        [SerializeField] private List<CrewController> crews = new List<CrewController>();
        [SerializeField] private List<CrewController> selectedCrews = new List<CrewController>();
        [SerializeField] private bool isPlayer = true;
        [SerializeField] private Room targetRoom = null;

        private void Awake()
        {
            CrewController[] crews = GetComponentsInChildren<CrewController>();
            for (int i = 0; i < crews.Length; i++)
            {
                this.crews.Add(crews[i]);
            }
            targetRoom = null;

            facilitySystems.Add(GetComponent<WeaponSystem>());
            facilitySystems.Add(GetComponent<EngineSystem>());
            facilitySystems.Add(GetComponent<ShieldSystem>());
        }

        //private void Update()
        //{
        //    // 조건에 맞으면 승무원 이동 로직 수행
        //    if (selectedCrews.Count > 0 &&
        //        targetRoom != null)
        //    {
        //    }
        //}

        private void SetTargetToCrew()
        {
            for (int i = 0; i < selectedCrews.Count; i++)
            {
                if (targetRoom.MaxCrewCapacity == targetRoom.CrewList.Count)
                {
                    Debug.LogWarning($"Room {targetRoom.Id} is full. Cannot move crew.");
                    break;
                }
                CrewController crew = selectedCrews[i];
                crew.SetTargetRoom(targetRoom);
            }

            //selectedCrews.Clear();
            targetRoom = null;
        }

        public void AddSelectedCrew(CrewController crew)
        {
            crew.IsSelected = true;
            selectedCrews.Add(crew);
        }

        public void ResetSelectedCrews()
        {
            int length = selectedCrews.Count;
            for (int i = 0; i < length; i++)
            {
                CrewController crew = selectedCrews[i];
                crew.IsSelected = false;
            }

            selectedCrews.Clear();
        }

        public void SetTargetRoom(Room room)
        {
            if (selectedCrews.Count == 0)
            {
                Debug.LogWarning($"Must select crew before choosing target room");
                return;
            }
            targetRoom = room;
            SetTargetToCrew();
        }
    }
}
