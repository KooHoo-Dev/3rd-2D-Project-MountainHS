namespace Assets._Scripts.Test
{
    public class Test
    {
        #region Public Method

        public void Main()
        {
            TestA testA = new();
            TestA test;
            TestB testB = new();
            test = testA;
            test = testB;
        }

        #endregion
    }
}
