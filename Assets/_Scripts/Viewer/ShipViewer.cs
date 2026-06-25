using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    /// <summary>
    /// 함선 내의 Room, RoomViewer 정보를 통해 시각화
    /// </summary>
    public class ShipViewer : MonoBehaviour
    {
        [SerializeField] private ShipController shipController;
        [SerializeField] private RoomViewer[] roomViewers;
        private ShipData shipData;

        private void Start()
        {
            shipData = shipController.ShipData;
            roomViewers = GetComponentsInChildren<RoomViewer>();
            List<Room> rooms = shipData.Rooms;
            for (int i = 0; i < rooms.Count; i++)
            {
                roomViewers[i].RoomData = rooms[i];
                roomViewers[i].SetRoomState();
            }
        }
    }
}
