using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using TelcobrightMediation;
using MediationModel;
namespace Utils
{

    public class OrderByTester
    {
        public void Test()
        {
            //List<string> randomStrs = GetRandomList().ToList();
            //randomStrs = randomStrs.OrderBy(s => s).ToList();
            //List<rate> rates = context.rates.Where(r => r.idrateplan == 37).ToList();
            List<Rateext> rates = JsonHelper.DeSerializeFromFile<List<Rateext>>(@"c:\temp\rates.json");
            Console.WriteLine("Entering orderby test...");
            Stopwatch st = new Stopwatch();


            //rates = rates.Sort(s=>s.Prefix)
            //rates = rates.OrderBy(r => r.field2).ThenByDescending(r => r.startdate).ToList();

            st.Start();
            //priority must be formatted to certain digit e.g. 0001, based on max length of priority
            //determine max length of priority then padd 0s accordingly
            DictionaryBasedRateSorter rateSorter = new DictionaryBasedRateSorter(rates,
                sortByPriorityDescending: false,
                sortByPrefixDescending: false,
                sortByStartDateDescending: true);
            rates = rateSorter.Sort();
            st.Stop();
            Console.WriteLine($"Elapsed time: {st.ElapsedMilliseconds / 1000} seconds. Press any key...");
            Console.ReadKey();
        }


        IEnumerable<string> GetRandomList()
        {
            for (int i = 0; i < 100000; i++)
            {
                yield return RandomStringGenerator.Next(10);
            }
        }
    }
}
