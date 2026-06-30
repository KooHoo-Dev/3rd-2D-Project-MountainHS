using System.Collections.Generic;

namespace FTL
{
    public class GameContext
    {
        public List<Room> Rooms;

        // 시설 상태, 전력 상태 저장용
        public List<FacilityState> FacilityStates;
        public List<CrewState> CrewStates;
        public int ScrapAmount;
        public int ShipHp;
        public int ShipReactorCapacity;

        public GameContext(List<Room> rooms, List<FacilitySystem> facilitySystems, List<CrewController> crews, int scrapAmount, int shipHp, int shipReactorCapacity)
        {

            FacilityStates = new List<FacilityState>();
            CrewStates = new List<CrewState>();
            for (int i = 0; i < facilitySystems.Count; i++)
            {
                FacilityStates.Add(new FacilityState(facilitySystems[i]));
            }
            for (int i = 0; i < crews.Count; i++)
            {
                CrewStates.Add(new CrewState(crews[i]));
            }
            
            Rooms = new List<Room>(rooms);
            ScrapAmount = scrapAmount;
            ShipHp = shipHp;
            ShipReactorCapacity = shipReactorCapacity;
        }

        public GameContext(List<Room> rooms, List<FacilityState> facilities, List<CrewState> crews, int scrapAmount, int shipHp, int shipReactorCapacity)
        {
            Rooms = new List<Room>();
            FacilityStates = new List<FacilityState>();
            CrewStates = new List<CrewState>();
            for (int i = 0; i < rooms.Count; i++)
            {
                Rooms.Add(new Room(rooms[i]));
            }
            for (int i = 0; i < facilities.Count; i++)
            {
                FacilityStates.Add(new FacilityState(facilities[i]));
            }
            for (int i = 0; i < crews.Count; i++)
            {
                CrewStates.Add(new CrewState(crews[i]));
            }

            ScrapAmount = scrapAmount;
            ShipHp = shipHp;
            ShipReactorCapacity = shipReactorCapacity;
        }

        //public void SetGameContext(List<Room> rooms,
        //    List<FacilityState> facilities,
        //    List<CrewState> crews,
        //    int shipHp, int scrapAmount, int shipReactorCapacity)
        //{
        //    Rooms = new List<Room>();
        //    FacilityStates = new List<FacilityState>();
        //    CrewStates = new List<CrewState>();
        //    for (int i = 0; i < rooms.Count; i++)
        //    {
        //        Rooms.Add(new Room(rooms[i]));
        //    }
        //    for (int i = 0; i < facilities.Count; i++)
        //    {
        //        FacilityStates.Add(new FacilityState(facilities[i]));
        //    }
        //    for (int i = 0; i < crews.Count; i++)
        //    {
        //        CrewStates.Add(new CrewState(crews[i]));
        //    }

        //    ShipHp = shipHp;
        //    ScrapAmount = scrapAmount;
        //    ShipReactorCapacity = shipReactorCapacity;

        //}

        public void SetGameContext(List<Room> rooms, List<FacilitySystem> facilitySystems, List<CrewController> crews, int scrapAmount, int shipHp, int shipReactorCapacity)
        {
            Rooms = new List<Room>();
            FacilityStates = new List<FacilityState>();
            CrewStates = new List<CrewState>();
            for (int i = 0; i < rooms.Count; i++)
            {
                Rooms.Add(new Room(rooms[i]));
            }
            for (int i = 0; i < facilitySystems.Count; i++)
            {
                FacilityStates.Add(new FacilityState(facilitySystems[i]));
            }
            for (int i = 0; i < crews.Count; i++)
            {
                CrewStates.Add(new CrewState(crews[i]));
            }

            ShipHp = shipHp;
            ScrapAmount = scrapAmount;
            ShipReactorCapacity = shipReactorCapacity;
        }
    }
}
