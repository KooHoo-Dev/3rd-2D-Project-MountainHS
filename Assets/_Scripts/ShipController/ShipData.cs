using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    // 함선의 체력, 타일 구조 정보를 관리한다
    [CreateAssetMenu(fileName = "ShipData", menuName = "SO/ShipData")]
    [System.Serializable]
    public class ShipData : ScriptableObject
    {
        // scriptable object로 Hp, Rooms 관리
        [field: SerializeField] public int MaxHp { get; private set; }
        [field: SerializeField] public List<RoomData> Rooms { get; private set; }
    }
}
