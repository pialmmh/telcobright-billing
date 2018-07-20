using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class ExceptionTester
    {
        public void Test()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    MockFunction();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    
                }
            }
        }

        private void MockFunction()
        {
            Func<int> fz= ()=> 0;
            var x = 2 / fz();
            Console.WriteLine("Line2: should not be displayed.");
        }
    }
}
