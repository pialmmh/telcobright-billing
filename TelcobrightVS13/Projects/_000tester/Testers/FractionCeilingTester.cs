using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using TelcobrightMediation.Accounting;

namespace Utils.Testers
{
    public class FractionCeilingTester
    {
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
                        new FractionCeilingHelper(amount, ceilingAtPosition);
                    
                    valAFterCeiling = fractionHelper.GetFormattedNumber();
                }
                catch (Exception e)
                {
                    valAFterCeiling = "Exception";
                }
                Console.WriteLine($@"Amount={amount}, ceilingPosition={ceilingAtPosition}, ValAfterCeiling={valAFterCeiling}");
            });
            
        }
    }
}
