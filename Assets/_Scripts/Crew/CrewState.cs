namespace FTL
{
    public class CrewState
    {
        public int Hp;
        public int MaxHp;
        public string Name;
        public int LastRoomId;

        public CrewState()
        {
            Hp = 0;
            Name = string.Empty;
            LastRoomId = -1;
            MaxHp = 0;
        }

        public CrewState(int maxHp, int hp, string name, int lastRoomId)
        {
            Hp = hp;
            Name = name;
            LastRoomId = lastRoomId;
            MaxHp = maxHp;
        }

        public CrewState(CrewState crewState)
        {
            Hp = crewState.Hp;
            MaxHp = crewState.MaxHp;
            Name = crewState.Name;
            LastRoomId = crewState.LastRoomId;
        }

        public CrewState(CrewController crew)
        {
            MaxHp = crew.MaxHp;
            Hp = crew.Hp;
            Name = crew.Name;
            LastRoomId = (crew.CurrentRoom != null) 
                ? crew.CurrentRoom.Id : -1;
        }
    }
}
