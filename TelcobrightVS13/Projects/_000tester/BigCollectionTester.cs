using System;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    class BigCollectionTester
    {
        public void Test()
        {
            var testList = new List<KeyValuePair<int, DateTime>>();
            foreach (int num in Enumerable.Range(1, 1000000))
            {
                testList.Add(new KeyValuePair<int, DateTime>(num, new DateTime()));
            }


        }
    }
}
