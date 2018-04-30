using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using TelcobrightMediation.Accounting;

namespace Utils.Testers
{
    public class FractionCeilingTester
    {
        private static readonly Random random = new Random();
        public void Test()
        {
            var numbers = new List<KeyValuePair<string,int>>()
            {
                //new KeyValuePair<string, int>("98",8),
                new KeyValuePair<string, int>("98",7),
                new KeyValuePair<string, int>("98.16003065M",7),
                new KeyValuePair<string, int>("98.06003065M",1),
                new KeyValuePair<string, int>("98.16003065M",1),
                new KeyValuePair<string, int>("0.16003065M",1),
                new KeyValuePair<string, int>("0.16003065M",2),
                new KeyValuePair<string, int>("0.16003065M",3),
                new KeyValuePair<string, int>("0.16003065M",4),
                new KeyValuePair<string, int>("0.16003065M",5),
                new KeyValuePair<string, int>("0.16003065M",6),
                new KeyValuePair<string, int>("0.16003065M",7),
                new KeyValuePair<string, int>("101.16003065M",1),
                new KeyValuePair<string, int>("101.16003065M",2),
                new KeyValuePair<string, int>("101.16003065M",3),
                new KeyValuePair<string, int>("101.16003065M",4),
                new KeyValuePair<string, int>("101.16003065M",5),
                new KeyValuePair<string, int>("101.16003065M",6),
                new KeyValuePair<string, int>("101.16003065M",7),
            };
            PrintAll(numbers);
        }

        private void PrintAll(List<KeyValuePair<string,int>> numbers)
        {
            int maxAllowedPrecissionByTB = 8;
            numbers.ForEach(kv =>
            {
                string valAFterCeiling = "";
                string amount = kv.Key;
                int ceilingAtPosition = kv.Value;
                try
                {
                    FractionCeilingHelper fractionHelper =
                        new FractionCeilingHelper(Convert.ToDecimal(amount), ceilingAtPosition);
                    
                    valAFterCeiling = fractionHelper.GetPreciseDecimal().ToString(CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    valAFterCeiling = "Exception";
                }
                Console.WriteLine($@"Amount={amount}, ceilingPosition={ceilingAtPosition}, ValAfterCeiling={valAFterCeiling}");
            });
            
        }

        public void PerformanceTest()
        {
            int count = 1000000;
            List<KeyValuePair< string, int>> numbers=new List<KeyValuePair<string, int>>();
            Random miniRandom = new Random();
            for (int i = 0; i < count; i++)
            {
                var rndDecimal = Convert.ToDecimal(RandomNumberBetween(1.41421, 120.14159));
                numbers.Add(new KeyValuePair<string, int>(rndDecimal.ToString(CultureInfo.InvariantCulture),
                    miniRandom.Next(1, 7)));
            }
            Stopwatch st=new Stopwatch();
            st.Reset();
            st.Start();
            var results = new List<KeyValuePair<string, string>>();
            for (var index = 0; index < numbers.Count; index++)
            {
                KeyValuePair<string, int> n = numbers[index];
                FractionCeilingHelper fractionHelper =
                    new FractionCeilingHelper(Convert.ToDecimal(n.Key), n.Value);
                results.Add(new KeyValuePair<string, string>(n.Key,
                    n.Value + "-> " + fractionHelper.GetPreciseDecimal()));
            }
            st.Stop();
            Console.WriteLine("Elapsed seconds: "+ st.ElapsedMilliseconds/1000);
            Console.Read();
        }

        
        

        private static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = random.NextDouble();

            return minValue + (next * (maxValue - minValue));
        }
    }
}
