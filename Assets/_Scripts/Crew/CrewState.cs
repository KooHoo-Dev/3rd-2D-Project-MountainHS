namespace FTL
{
    public class CrewState
    {
        public int Hp;
        public string Name;
        public int LastRoomId;

        public CrewState()
        {
            Hp = 0;
            Name = string.Empty;
            LastRoomId = -1;
        }

        public CrewState(int hp, string name, int lastRoomId)
        {
            Hp = hp;
            Name = name;
            LastRoomId = lastRoomId;
        }

        public CrewState(CrewState crewState)
        {
            Hp = crewState.Hp;
            Name = crewState.Name;
            LastRoomId = crewState.LastRoomId;
        }

        public CrewState(CrewController crew)
        {
            Hp = crew.Hp;
            Name = crew.Name;
            LastRoomId = (crew.CurrentRoom != null) 
                ? crew.CurrentRoom.Id : -1;
        }
    }
}
