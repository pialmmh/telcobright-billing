using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediationModel;

namespace TelcobrightMediation
{
    public class UoMConvRateCache : AbstractCache<uom_conversion_dated,string>//string=acc name
    {
        public UoMConvRateCache(Func<uom_conversion_dated, string> dictionaryKeyGenerator,
            Func<uom_conversion_dated, StringBuilder> insertCommandGenerator,
            Func<uom_conversion_dated, StringBuilder> updateWherePartGenerator,
            Func<uom_conversion_dated, StringBuilder> deleteommandGenerator)
            : base(dictionaryKeyGenerator, insertCommandGenerator, updateWherePartGenerator,deleteommandGenerator) { }//pass to base
        public override void PopulateCache(Func<Dictionary<string, uom_conversion_dated>> methodToPopulate)
        {
            foreach (var keyValuePair in methodToPopulate.Invoke())
            {
                if(base.Cache.TryAdd(keyValuePair.Key, keyValuePair.Value)==false)
                    throw new Exception("Could not add usdRate to to concurrent dictionary while populating UoMConvRateCache, probably duplicate item.");
            }
        }
        public uom_conversion_dated GetNearestEarlierDateTime(DateTime inputDate)
        {
            List<DateTime> allDates = this.Cache.Values.Select(c=>c.FROM_DATE)
                                        .OrderBy(c => c).ToList();//ascending req to find nearest date
            if (allDates.Any() == false) return null;
            DateTime? closestDate = inputDate >= allDates.Last()
                ? allDates.Last()
                : inputDate < allDates.First()
                    ? (DateTime?)null
                    : allDates.FirstOrDefault(d => d < inputDate);
            if (closestDate == null) return null;
            string dicKey = this.DictionaryKeyGenerator(new uom_conversion_dated()
            {
                UOM_ID = "USD",
                UOM_ID_TO = "BDT",
                FROM_DATE = Convert.ToDateTime(closestDate)
            });
            return this.Cache[dicKey];
        }
    }
}