using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation
{
    public class RateSorter
    {
        private List<Rateext> rates;
        Dictionary<string, List<Rateext>> keyWiseRates = new Dictionary<string, List<Rateext>>();
        public RateSorter(List<Rateext> _rates, Func<Rateext, string> keyGenerator)
        {
            this.rates = _rates;
            this.rates.ForEach(r =>
            {
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

    public class DictionaryBasedRateSorter
    {
        private bool sortByPriorityDescending = false;
        private bool sortByPrefixDescending = false;
        private bool sortByStartDateDescending = false;
        private List<Rateext> rates;
        Dictionary<int,Dictionary<string,List<Rateext>>> rateDictionary=new Dictionary<int, Dictionary<string, List<Rateext>>>();
        public DictionaryBasedRateSorter(List<Rateext> _rates, bool sortByPriorityDescending, 
            bool sortByPrefixDescending, bool sortByStartDateDescending)
        {
            this.rates = _rates;
            this.sortByPriorityDescending = sortByPriorityDescending;
            this.sortByPrefixDescending = sortByPrefixDescending;
            this.sortByStartDateDescending = sortByStartDateDescending;
            this.rates.ForEach(r =>
            {
                Dictionary<string, List<Rateext>> prefixWiseRates = null;
                int priority = Convert.ToInt32(r.Priority);
                rateDictionary.TryGetValue(priority, out prefixWiseRates);
                if (prefixWiseRates==null)
                {
                    prefixWiseRates=new Dictionary<string, List<Rateext>>();
                    rateDictionary.Add(priority,prefixWiseRates);
                }
                List<Rateext> rateListInnerMost=prefixWiseRates.AppendAndGetListIfMissing(r.Prefix);
                rateListInnerMost.Add(r);
            });
        }

        public List<Rateext> Sort()
        {
            this.rates.Clear();
            List<int> sortedPriorities = this.rateDictionary.Keys.ToList();
            sortedPriorities.Sort();
            if (this.sortByPriorityDescending==true)
            {
                sortedPriorities.Reverse();
            }
            sortedPriorities.ForEach(p =>
            {
                Dictionary<string, List<Rateext>> prefixWiseRates = null;
                rateDictionary.TryGetValue(p, out prefixWiseRates);
                List<string> sortedPrefixes = prefixWiseRates.Keys.ToList();
                sortedPrefixes.Sort();
                if (this.sortByPrefixDescending==true)
                {
                    sortedPrefixes.Reverse();
                }
                sortedPrefixes.ForEach(prefix =>
                {
                    List<Rateext> innerRates = null;
                    prefixWiseRates.TryGetValue(prefix, out innerRates);
                    if (this.sortByStartDateDescending)
                    {
                        innerRates = innerRates.OrderByDescending(r => r.P_Startdate).ToList();
                    }
                    else
                    {
                        innerRates = innerRates.OrderBy(r => r.P_Startdate).ToList();
                    }
                    this.rates.AddRange(innerRates);
                });
            });
            return this.rates;
        }
    }
}
