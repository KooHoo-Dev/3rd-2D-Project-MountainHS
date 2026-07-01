using System;
using System.Collections.Generic;
using UnityEngine;

namespace FTL
{
    [Serializable]
    public class GameContext
    {
        public bool IsEmpty => Rooms == null || FacilityStates == null || CrewStates == null;

        public List<Room> Rooms { get; private set; }
        public List<FacilityState> FacilityStates { get; private set; }
        public List<CrewState> CrewStates { get; private set; }
        [field: SerializeField] public List<int> FacilityUpgradeCount { get; private set; } = new List<int>();
        [field: SerializeField] public int ScrapAmount { get; private set; }
        [field: SerializeField] public int ShipMaxHp { get; private set; }
        [field: SerializeField] public int ShipHp { get; private set; }
        [field: SerializeField] public int ShipReactorCapacity { get; private set; }
        [field: SerializeField] public int ReactorUpgradeCount { get; private set; }

        public GameContext(List<Room> rooms, List<FacilitySystem> facilitySystems, List<CrewController> crews, int scrapAmount, int shipHp, int shipMaxHp, int shipReactorCapacity)
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
            ShipMaxHp = shipMaxHp;
            ShipReactorCapacity = shipReactorCapacity;
        }

        public GameContext(List<Room> rooms, List<FacilityState> facilities, List<CrewState> crews, int scrapAmount, int shipHp, int shipMaxHp, int shipReactorCapacity)
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
            ShipMaxHp = shipMaxHp;
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

        public void SetGameContext(List<Room> rooms, List<FacilitySystem> facilitySystems, List<CrewController> crews, int scrapAmount, int shipHp, int shipMaxHp, int shipReactorCapacity)
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
            ShipMaxHp = shipMaxHp;
            ScrapAmount = scrapAmount;
            ShipReactorCapacity = shipReactorCapacity;
        }

        public void SetGameContext(List<int> facilityUpgradeCount, int reactorUpgradeCount)
        {
            for (int i = 0; i < FacilityUpgradeCount.Count; i++)
            {
                FacilityUpgradeCount[i] = facilityUpgradeCount[i];
            }

            ReactorUpgradeCount = reactorUpgradeCount;
        }


        public void IncreaseMaxPowerLimitOfFacility(ERoomType roomType, int amount = 1)
        {
            int index = (int)roomType;
            FacilityStates[index].MaxPowerLimit += amount;
        }

        public void IncreaseReactorMaxPower(int amount)
        {
            ShipReactorCapacity+= amount;
        }

        public int GetCrewHp(int crewIndex)
        {
            return CrewStates[crewIndex].Hp;
        }

        public int GetCrewMaxHp(int crewIndex)
        {
            return CrewStates[crewIndex].MaxHp;
        }

        public int GetCrewCount()
        {
            return CrewStates.Count;
        }

        public string GetCrewName(int crewIndex)
        {
            return CrewStates[crewIndex].Name;
        }

        public int GetFacilityMaxPowerLimit(ERoomType roomType)
        {
            int index = (int)roomType;

            return FacilityStates[index].MaxPowerLimit;
        }

        //public int GetShipReactorCapacity()
        //{
        //    return ShipReactorCapacity;
        //}

        public void AddCrewHp(int crewIndex, int realIncrease)
        {
            CrewStates[crewIndex].Hp += realIncrease;
        }

        public void AddShipHp(int amount)
        {
            ShipHp = Math.Min(ShipMaxHp, ShipHp + amount);
        }

        public void ReduceScrap(int v)
        {
            if (ScrapAmount - v < 0)
            {
                Debug.LogError($"Cannot Reduce scrap. current scrap: {ScrapAmount}, reduce amount: {v}");
                return;
            }

            ScrapAmount -= v;
        }

        public void AddScrap(int v)
        {
            ScrapAmount += v;
        }
    }
}
