using System;
using System.Collections.Generic;
using System.Text;

namespace Assets._Scripts.Test
{
    public class Test
    {
        #region Public Method

        public void Main()
        {
            TestA testA = new TestA();
            TestA test;
            TestB testB = new TestB();
            test = testA;
            test = testB;
        }

        #endregion
    }
}
