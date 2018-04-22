using LibraryExtensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using TelcobrightMediation.Accounting;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class XyzRuleHelper
    {
        public PrefixMatcher PrefixMatcher { get; set; }
        IServiceFamily Sf { get; set; }
        private UoMConvRateCache UsdBcsCache { get;}
        public XyzRuleHelper(UoMConvRateCache usdBcsCache, PrefixMatcher prefixMatcher, IServiceFamily sf)
        {
            this.UsdBcsCache = usdBcsCache;
            this.PrefixMatcher = prefixMatcher;
            this.Sf = sf;
        }

        private account GetPostingAccount(CdrExt newCdrExt, ServiceContext serviceContext, string idCurrencyUoM)
        {
            account postingAccount = null;
            AccountFactory accountFactory=new AccountFactory(serviceContext.CdrProcessor.CdrJobContext.AccountingContext);
            postingAccount = accountFactory.CreateOrGetBillable(0, serviceContext.IdServiceGroup, Convert.ToInt32(newCdrExt.Cdr.inPartnerId),
                serviceContext.ServiceFamily.Id,0, idCurrencyUoM);
            return postingAccount;
        }

        public AccChargeableExt ExecuteXyzRating(Rateext matchedRateWithAssignmentTupleId, CdrExt cdrExt, ServiceContext serviceContext,
            XyzRatingType xyzRatingType)
        {
            cdr thisCdr = cdrExt.Cdr;
            long finalDuration = 0; //xyz is rounded always
            decimal tempDuration = thisCdr.DurationSec;
            finalDuration =
                Convert.ToInt64(this.PrefixMatcher.A2ZDuration(tempDuration, matchedRateWithAssignmentTupleId));
            decimal xAmountBdt = this.PrefixMatcher.A2ZAmount(finalDuration, matchedRateWithAssignmentTupleId,
                rateFieldNumber: 0,//rate amount
                cdrProcessor: serviceContext.CdrProcessor);
            decimal yAmountUsd = this.PrefixMatcher.A2ZAmount(finalDuration, matchedRateWithAssignmentTupleId, 
                rateFieldNumber: 1,//other amount1
                cdrProcessor: serviceContext.CdrProcessor);
            decimal btrcRevSharePercentage = Convert.ToDecimal(matchedRateWithAssignmentTupleId.OtherAmount3);
            thisCdr.roundedduration = finalDuration;
            thisCdr.SubscriberChargeXOut = Convert.ToDecimal(xAmountBdt);
            thisCdr.CarrierCostYIGWOut = Convert.ToDecimal(yAmountUsd);
            thisCdr.MatchedPrefixY = matchedRateWithAssignmentTupleId.Prefix;
            thisCdr.matchedprefixcustomer = thisCdr.MatchedPrefixY;
            thisCdr.CountryCode = matchedRateWithAssignmentTupleId.CountryCode;
            thisCdr.CustomerRate = matchedRateWithAssignmentTupleId.OtherAmount1;
           
            //add the 100ms part 
            decimal duration100 = this.PrefixMatcher.HundredMsDuration(tempDuration);
            thisCdr.Duration3 = duration100;
            DateTime callDate = thisCdr.StartTime;
            uom_conversion_dated conversionRate = GetExactOrNearestEarlierConvRateForXyz(callDate);
            if (conversionRate == null) return null;
            thisCdr.USDRateY = Convert.ToDecimal(conversionRate.CONVERSION_FACTOR);
            decimal yBdt = yAmountUsd * (decimal)thisCdr.USDRateY;
            decimal zAmount = xAmountBdt- yBdt;
            decimal fifteenPcOfZ = Convert.ToDecimal(zAmount * matchedRateWithAssignmentTupleId.OtherAmount2) / 100;
            decimal finalAmount = xyzRatingType == XyzRatingType.Igw ? fifteenPcOfZ + yBdt : fifteenPcOfZ;
            thisCdr.RevenueIGWOut = finalAmount;
            account postingAccount = GetPostingAccount(cdrExt, serviceContext, "BDT");
            BillingRule billingRule = GetBillingRule(serviceContext,
                matchedRateWithAssignmentTupleId.IdRatePlanAssignmentTuple);
            if (cdrExt.Cdr.ChargingStatus == 1)
            {
                var chargeable = new acc_chargeable()
                {
                    id =
                        serviceContext.CdrProcessor.CdrJobContext.AccountingContext.AutoIncrementManager
                            .GetNewCounter("acc_chargeable"),
                    uniqueBillId = thisCdr.UniqueBillId,
                    idEvent = Convert.ToInt64(thisCdr.idcall),
                    transactionTime = callDate,
                    servicegroup = serviceContext.IdServiceGroup,
                    assignedDirection =
                        1, //chared to customer, although does not have assigndir in rateplanassigntuple table
                    servicefamily = serviceContext.ServiceFamily.Id,
                    ProductId = matchedRateWithAssignmentTupleId.ProductId,
                    idBilledUom = "BDT",
                    idQuantityUom = "TF_s", //seconds
                    BilledAmount = Convert.ToDecimal(finalAmount),
                    Quantity = finalDuration,
                    OtherAmount1 = xAmountBdt,
                    OtherAmount2 = yAmountUsd,
                    OtherAmount3 = zAmount,
                    OtherDecAmount1 = fifteenPcOfZ*btrcRevSharePercentage/100,
                    unitPriceOrCharge = matchedRateWithAssignmentTupleId.rateamount,
                    Prefix = matchedRateWithAssignmentTupleId.Prefix,
                    RateId = matchedRateWithAssignmentTupleId.id,
                    glAccountId = postingAccount.id,
                    jsonDetail = new StringBuilder("{\"usdRate\":\"").Append(conversionRate.CONVERSION_FACTOR.ToString())
                        .Append("\"}").ToString(),
                    changedByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id,
                    idBillingrule = billingRule.Id,
                };
                chargeable.description =
                    new List<string> {"1", "2"}.Contains(serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob
                        .idjobdefinition.ToString())
                        ? "nc" //use new cdr flag for new & error cdr job
                        : serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.idjobdefinition.ToString() == "3"
                            ? "rc" //reprocess cdr
                            : "unknown";
                if (chargeable.createdByJob == null)
                {
                    chargeable.createdByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id;
                }
                return new AccChargeableExt(chargeable)
                {
                    RateExt = matchedRateWithAssignmentTupleId,
                    Account = postingAccount
                };
            }
            return null;
        }

        private BillingRule GetBillingRule(ServiceContext serviceContext, long idRatePlanAssignmentTuple)
        {
            int idbillingRule = Convert.ToInt32(serviceContext.MefServiceFamilyContainer
                .IdWiseRateplanAssignmenttuplesIncludingBillingRules[idRatePlanAssignmentTuple.ToString()]
                .billingruleassignment.idBillingRule);
            BillingRule billingRule = serviceContext.MefServiceFamilyContainer.BillingRules[idbillingRule.ToString()];
            return billingRule;
        }

        private uom_conversion_dated GetExactOrNearestEarlierConvRateForXyz(DateTime callDate)
        {
            var year = callDate.Year;
            var month = callDate.Month-1;
            DateTime lastMonthsUsdbBcsDateTime = new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
            CachedItem<string, uom_conversion_dated> convRate = null;
            string dicKey =
                this.UsdBcsCache.DictionaryKeyGenerator(
                    new uom_conversion_dated()
                    {
                        UOM_ID = "USD",
                        UOM_ID_TO = "BDT",
                        FROM_DATE = lastMonthsUsdbBcsDateTime
                    });
            convRate = this.UsdBcsCache.GetItemByKey(dicKey); //exact match, last months's data at 23:59:59
            if (convRate != null)
            {
                return convRate.Entity;
            }
            else return this.UsdBcsCache.GetNearestEarlierDateTime(lastMonthsUsdbBcsDateTime);
        }
    }
}
