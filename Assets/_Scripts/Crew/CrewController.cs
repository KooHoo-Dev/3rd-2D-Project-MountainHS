using UnityEngine;

namespace FTL
{
    public class CrewController : MonoBehaviour
    {
        [field: SerializeField] public Room CurrentRoom { get; set; } = null;
        [field: SerializeField] public Room TargetRoom { get; private set; } = null;
        public int Hp { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0.5f, 0f);


        private void Update()
        {
            MoveToTargetRoom();
        }

        public void MoveToTargetRoom()
        {
            if (TargetRoom?.IsUnintialized() == false)
            {
                if ((Vector2)transform.position == TargetRoom.GlobalPosition)
                {
                    //CurrentRoom = TargetRoom;
                    TargetRoom = null;
                }
                else
                {
                    Vector3 targetPosition = TargetRoom.GlobalPosition + (Vector2)(offset * (TargetRoom.CrewList.IndexOf(this)));
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                }
            }
        }

        public void SetTargetRoom(Room room)
        {
            TargetRoom = room;
            CurrentRoom.ReduceCrew(this);
            TargetRoom.AddCrew(this);
            CurrentRoom = TargetRoom;
        }

        public void InitStartRoom(Room room)
        {
            CurrentRoom = room;
            room.AddCrew(this);
            transform.position = room.GlobalPosition;
        }
    }
}
