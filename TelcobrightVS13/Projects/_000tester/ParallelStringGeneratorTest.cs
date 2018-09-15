using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class ParallelStringGeneratorTest
    {
        private int noOfExec = 1000;
        public void Test()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < this.noOfExec; i++)
            {
                var numbersNp = nonParallel();
            }
            stopwatch.Stop();
            Console.WriteLine("Nonparallel EXEC TIME {0}", stopwatch.ElapsedMilliseconds);
            stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < this.noOfExec; i++)
            {
                var numbersNp = parallel();
            }
            stopwatch.Stop();
            Console.WriteLine("Parallel EXEC TIME {0}", stopwatch.ElapsedMilliseconds);
            Console.Read();
        }

        private string[] nonParallel()
        {
            var phoneNumber = "12345678912";
            var phCharArray = phoneNumber.ToCharArray();
            var phoneNumbersAsArray = new string[phoneNumber.Length];
            for (int i = 0; i < phCharArray.Length; i++)
            {
                phoneNumbersAsArray[i] = new string(phCharArray, 0, phCharArray.Length - i);
            }
            return phoneNumbersAsArray;
        }

        private string[] parallel()
        {
            var phoneNumber = "12345678912";
            var phCharArray = phoneNumber.ToCharArray();
            var phoneNumbersAsArray = new string[phoneNumber.Length];
            Parallel.For((int) 0, phCharArray.Length, i =>
            {
                phoneNumbersAsArray[i] = new string(phCharArray, 0, phCharArray.Length - i);
            });
            return phoneNumbersAsArray;
        }
    }
}
