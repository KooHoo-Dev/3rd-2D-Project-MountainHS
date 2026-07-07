using System;
using TMPro;
using UnityEngine;

namespace FTL
{
    public class RoomViewer : MonoBehaviour
    {
        [field: SerializeField] public Room Room { get; set; }
        private SpriteRenderer spriteRenderer;
        private SpriteShakeController spriteShaker;
        private TMP_Text text;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteShaker = GetComponent<SpriteShakeController>();
            text = GetComponentInChildren<TMP_Text>();
            SetRoomState(Room);
        }

        private void Update()
        {
            if (Room.IsHit && spriteShaker.IsShaked == false)
            {
                spriteShaker.ShakeSprite();
            }
        }

        /// <summary>
        /// 외부에서 roomId, roomType 입력받으면 그에 맞게 텍스트, 색깔 바꾸기
        /// </summary>
        public void SetRoomState(int id, RoomType type)
        {
            text.SetText($"{id}");
            switch (type)
            {
                case RoomType.Empty:
                    spriteRenderer.color = Color.white;
                    break;
                case RoomType.Weapon:
                    spriteRenderer.color = Color.red;
                    break;
                case RoomType.Shield:
                    spriteRenderer.color = Color.blue;
                    break;
                case RoomType.Engine:
                    spriteRenderer.color = Color.green;
                    break;
            }
        }

        public void SetRoomState(Room room)
        {
            if (room == null)
            {
                return;
            }

            SetRoomState(room.Id, room.Type);
        }

        public void SetRoomState()
        {
            SetRoomState(Room.Id, Room.Type);
        }
    }
}
