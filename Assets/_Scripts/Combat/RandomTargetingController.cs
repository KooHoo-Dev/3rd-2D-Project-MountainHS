using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    // 랜덤 방정보를 제공하는 클래스
    public class RandomTargetingController: MonoBehaviour, ITargeting
    {
        [SerializeField] private ShipController ship;
        [SerializeField] private List<RoomData> rooms = new List<RoomData>();

        private void Start()
        {
            for (int i = 0; i < ship.Rooms.Count; i++)
            {
                rooms.Add(ship.Rooms[i]);
            }
        }

        public RoomData GetTarget()
        {
            int randomIndex = Random.Range(0, rooms.Count);
            return rooms[randomIndex];
        }
    }
}
