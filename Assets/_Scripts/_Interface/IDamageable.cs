namespace FTL
{
    public interface IDamageable
    {
        #region Public Method

        void TakeDamage(int damage);

        void RestoreHp(int damage);

        #endregion
    }
}
