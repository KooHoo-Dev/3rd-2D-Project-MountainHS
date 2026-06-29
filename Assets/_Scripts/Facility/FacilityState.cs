namespace FTL
{
    public class FacilityState
    {
        public ERoomType Type;
        public int TotalDamage;
        public int MaxPowerLimit;
        public int Power;

        public FacilityState(int totalDamage, int maxPowerLimit, int power, ERoomType facilityType)
        {
            TotalDamage = totalDamage;
            MaxPowerLimit = maxPowerLimit;
            Power = power;
            Type = facilityType;
        }

        public FacilityState(FacilityState facilityState)
        {
            TotalDamage = facilityState.TotalDamage;
            MaxPowerLimit = facilityState.MaxPowerLimit;
            Power = facilityState.Power;
            Type = facilityState.Type;
        }

        public FacilityState(FacilitySystem facility)
        {
            TotalDamage = facility.TotalDamage;
            MaxPowerLimit = facility.MaxPowerLimit;
            Power = facility.Power;
            Type = facility.Type;
        }
    }
}
