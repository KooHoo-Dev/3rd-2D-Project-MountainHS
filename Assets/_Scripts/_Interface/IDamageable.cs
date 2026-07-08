namespace FTL
{
    public interface IDamageable
    {
        #region Public Method

        public void TakeDamage(int damage);

        public void RestoreHp(int damage);

        #endregion
    }
}
