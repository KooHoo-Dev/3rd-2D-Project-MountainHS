using System.Collections;

namespace FTL
{
    public interface IDeadable
    {
        #region Public Method

        public IEnumerator DeadCoroutine(float seconds, BattleResult result);

        #endregion
    }
}
