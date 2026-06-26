using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    // 랜덤 방정보를 제공하는 클래스
    public class RandomTargeter: ITargeting
    {
        private List<RoomData> rooms = new List<RoomData>();

        public RandomTargeter(List<RoomData> rooms)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                this.rooms.Add(rooms[i]);
            }
        }

        public RoomData GetTarget()
        {
            int randomIndex = Random.Range(0, rooms.Count);
            return rooms[randomIndex];
        }
    }
}
