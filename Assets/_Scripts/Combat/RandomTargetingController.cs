using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    // 랜덤 방정보를 제공하는 클래스
    public class RandomTargetingController: MonoBehaviour, ITargeting
    {
        [SerializeField] private ShipController ship;
        [SerializeField] private List<RoomViewer> roomViewers = new List<RoomViewer>();

        private void Start()
        {
            for (int i = 0; i < ship.RoomViewers.Count; i++)
            {
                roomViewers.Add(ship.RoomViewers[i]);
            }
        }

        public RoomViewer GetTarget()
        {
            int randomIndex = Random.Range(0, roomViewers.Count);
            return roomViewers[randomIndex];
        }
    }
}
