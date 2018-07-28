using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LibraryExtensions;
using MediationModel;
using TelcobrightMediation.Accounting;

namespace TelcobrightMediation
{
    public class MefServiceFamilyContainer
    {
        public Dictionary<ValueTuple<int,string>, route> DicRouteIncludingPartner { get; set; }//<switchid-route,route>
        public ServiceFamilyComposer CmpServiceFamily { get; }
        public IDictionary<int, IServiceFamily> DicExtensions { get; }
        public RateCache RateCache { get; set; }
        public Dictionary<int, rateplanassignmenttuple> IdWiseRateplanAssignmenttuplesIncludingBillingRules { get; set; }
        public Dictionary<int,TupleDefinitions> ServiceGroupWiseTupDefs { get; set; }
        public Dictionary<int, rateplan> DicRateplans { get; set; }//to fetch currency & other infor during rating
        public Dictionary<int,BillingRule> BillingRules { get; set; }
        public UoMConvRateCache UsdBcsCache { get; private set; }
        public MefServiceFamilyContainer()
        {
            this.DicRouteIncludingPartner = new Dictionary<ValueTuple<int, string>, route>();
            this.CmpServiceFamily=new ServiceFamilyComposer();
            this.DicExtensions=new Dictionary<int, IServiceFamily>();
            this.RateCache = null;
            this.DicRateplans=new Dictionary<int, rateplan>();
            this.BillingRules=new Dictionary<int, BillingRule>();
            this.ServiceGroupWiseTupDefs=new Dictionary<int, TupleDefinitions>();
        }
        public void PopulateCachedUsdBcs(PartnerEntities context)
        {
            Func<uom_conversion_dated, string> dictionaryKeyGenerator =
                e => string.Join("^", new string[] { e.UOM_ID, e.UOM_ID_TO, e.FROM_DATE.ToMySqlField() });
            this.UsdBcsCache = new UoMConvRateCache(dictionaryKeyGenerator, //usd BcSelling cache,
                null, null, null); //no insert or update for usd bcs 
            this.UsdBcsCache.PopulateCache(() =>
            {
                Dictionary<string, uom_conversion_dated> dic = new Dictionary<string, uom_conversion_dated>();
                //if multiple conversion rates exist at the same datetime, add only the first, latest will be added due to desc ordering
                context.uom_conversion_dated.Where(c => c.UOM_ID == "USD" && c.UOM_ID_TO == "BDT"
                                                        && c.PURPOSE_ENUM_ID == "EXTERNAL_CONVERSION")
                    .OrderByDescending(c => c.FROM_DATE).ToList()
                    .ForEach(u =>
                    {
                        string dicKey = dictionaryKeyGenerator(u);
                        if (dic.ContainsKey(dicKey) == false) dic.Add(dicKey, u);
                    });
                return dic;
            });
        }
    }
}