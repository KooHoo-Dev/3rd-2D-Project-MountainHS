using UnityEngine;

namespace FTL
{
    public class ReactorSystem : MonoBehaviour, IPowerInfo
    {
        public int MaxPower => maxPower;
        public int Power => power;
        public int TotalDamage => 0;

        [SerializeField] private int maxPower = 3;
        [SerializeField] private int power;

        private void Awake()
        {
            power = maxPower;
        }

        public void AddPower(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            power = Mathf.Min(power + amount, maxPower);
        }

        public void AddMaxPower(int amount)
        {
            maxPower += amount;
            power += amount;
        }

        public void ReducePower(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            power = Mathf.Max(power - amount, 0);
        }
    }
}
