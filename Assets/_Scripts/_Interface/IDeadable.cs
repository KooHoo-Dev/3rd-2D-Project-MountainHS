using System.Collections;

namespace FTL
{
    public interface IDeadable
    {
        public IEnumerator DeadCoroutine(float seconds, EBattleResult result);
    }
}
