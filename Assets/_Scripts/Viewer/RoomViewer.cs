using TMPro;
using UnityEngine;

namespace FTL
{
    public class RoomViewer : MonoBehaviour
    {
        [field: SerializeField] public RoomData RoomData { get; set; }
        private SpriteRenderer spriteRenderer;
        private TMP_Text text;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            text = GetComponentInChildren<TMP_Text>();
            SetRoomState(RoomData);
        }

        /// <summary>
        /// 외부에서 roomId, roomType 입력받으면 그에 맞게 텍스트, 색깔 바꾸기
        /// </summary>
        public void SetRoomState(int id, ERoomType type)
        {
            text.SetText($"{id}");
            switch (type)
            {
                case ERoomType.Empty:
                    spriteRenderer.color = Color.white;
                    break;
                case ERoomType.Weapon:
                    spriteRenderer.color = Color.red;
                    break;
                case ERoomType.Shield:
                    spriteRenderer.color = Color.blue;
                    break;
                case ERoomType.Engine:
                    spriteRenderer.color = Color.green;
                    break;
            }
        }

        public void SetRoomState(RoomData roomData)
        {
            if (roomData == null)
            {
                return;
            }

            SetRoomState(roomData.Id, roomData.Type);
        }

        public void SetRoomState()
        {
            SetRoomState(RoomData.Id, RoomData.Type);
        }
    }
}
