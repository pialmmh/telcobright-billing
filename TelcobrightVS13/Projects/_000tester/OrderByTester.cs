using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;

namespace Utils
{
    public class ObjectWrapperWithCustomKey<T, TKey> where T : class
    {
        public TKey Key { get; }
        public T Obj { get; }

        public ObjectWrapperWithCustomKey(T obj, Func<T, TKey> keyGenerator)
        {
            Obj = obj;
            this.Key = keyGenerator.Invoke(obj);
        }
    }

    class RateSorter
    {
        private List<Rateext> rates;
        Dictionary<string, List<Rateext>> keyWiseRates = new Dictionary<string, List<Rateext>>();
        public RateSorter(List<Rateext> _rates)
        {
            this.rates = _rates;
            string maxPriorityAsStr = this.rates.Max(r => r.Priority).ToString();

            this.rates.ForEach(r =>
            {//todo: priority must be formatted to certain digit e.g. 0001, based on max length of priority
                //determine max length of priority then padd 0s accordingly
                Func<Rateext, string> keyGenerator = r1 =>
                    new StringBuilder(r1.Priority.ToString()).Append("/").Append(r1.Prefix).Append("/")
                        .Append(Convert.ToDateTime(r1.startdate).ToMySqlStyleDateTimeStrWithoutQuote()).ToString();
                ObjectWrapperWithCustomKey<Rateext, string> rateWithKey =
                    new ObjectWrapperWithCustomKey<Rateext, string>(r, keyGenerator);
                var innerRateList = keyWiseRates.AppendAndGetListIfMissing(rateWithKey.Key);
                innerRateList.Add(r);
            });
        }

        public List<Rateext> Sort()
        {
            List<string> sortedKeys = this.keyWiseRates.Keys.ToList();
            sortedKeys.Sort();
            this.rates.Clear();
            sortedKeys.ForEach(k =>
            {
                this.rates.AddRange(this.keyWiseRates[k]);
            });
            return this.rates;
        }
    }
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
            //rates = rates.OrderBy(r => r.field2).ThenBy(r => r.Prefix).ThenByDescending(r => r.startdate).ToList();
            //subRates = subRates.OrderBy(r => r.Field2,).ThenBy(r => r.Prefix).ThenByDescending(r => r.StartTime).ToList();
            RateSorter rateSorter=new RateSorter(rates);
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
    class Rateext : rate
    {
        public int IdRatePlanAssignmentTuple { get; set; }
        public override string ToString()
        {
            return new StringBuilder().Append(this.Prefix).Append("/")
                .Append(this.id)
                .Append("/")
                .Append(this.P_Startdate != null ? Convert.ToDateTime(this.P_Startdate).ToString("yyyy-MM-dd HH:mm:ss") : "null")
                .Append("/")
                .Append(this.P_Enddate != null ? Convert.ToDateTime(this.P_Enddate).ToString("yyyy-MM-dd HH:mm:ss") : "null").ToString();
        }

        public int? Priority { get; set; }
        public int AssignmentFlag { get; set; }
        private DateTime? Enddatebyrateplan { get; set; }
        private DateTime? Startdatebyrateplan { get; set; }

        private int OpenRateAssignment { get; set; }

        public int IdPartner { get; set; }
        public int IdRoute { get; set; }
        public string TechPrefix { get; set; }
        public string Pcurrency { get; set; }
        public string PrefixWithTechPrefix(ref Dictionary<string, rateplan> dicRatePlan)
        {
            return dicRatePlan[this.idrateplan.ToString()].field4 + this.Prefix;
        }

        public DateTime? P_Startdate
        {
            get
            {
                if (this.AssignmentFlag == 0)
                    return this.startdate;
                return this.startdate >= this.Startdatebyrateplan ? this.startdate : this.Startdatebyrateplan;
            }
        }

        public DateTime? P_Enddate
        {
            get
            {
                if (this.AssignmentFlag == 0)
                    return this.enddate;

                if (this.OpenRateAssignment == 1)//the rate's rateplan assignment is open
                {
                    if (this.enddate == null)
                        return null;
                    else//enddate not null
                        return this.enddate;
                }
                else//rateplan assignment has an enddate, NOT OPEN
                {
                    if (this.enddate == null)
                        return this.Enddatebyrateplan;
                    else//enddate not null
                        return this.enddate <= this.Enddatebyrateplan ? this.enddate : this.Enddatebyrateplan;
                }
            }
        }
    }
}
