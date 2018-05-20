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
        private UoMConvRateCache UsdBcsCache { get; }

        public XyzRuleHelper(UoMConvRateCache usdBcsCache, PrefixMatcher prefixMatcher, IServiceFamily sf)
        {
            this.UsdBcsCache = usdBcsCache;
            this.PrefixMatcher = prefixMatcher;
            this.Sf = sf;
        }

        private account GetPostingAccount(CdrExt newCdrExt, ServiceContext serviceContext, string idCurrencyUoM)
        {
            account postingAccount = null;
            AccountFactory accountFactory =
                new AccountFactory(serviceContext.CdrProcessor.CdrJobContext.AccountingContext);
            postingAccount = accountFactory.CreateOrGetBillable(0, serviceContext.ServiceGroupConfiguration.IdServiceGroup,
                Convert.ToInt32(newCdrExt.Cdr.InPartnerId),
                serviceContext.ServiceFamily.Id, 0, idCurrencyUoM);
            return postingAccount;
        }

        public AccChargeableExt ExecuteXyzRating(Rateext matchedRateWithAssignmentTupleId, CdrExt cdrExt,
            ServiceContext serviceContext,
            XyzRatingType xyzRatingType)
        {
            cdr thisCdr = cdrExt.Cdr;
            long finalDuration = 0; //xyz is rounded always
            decimal tempDuration = thisCdr.DurationSec;
            finalDuration =
                Convert.ToInt64(this.PrefixMatcher.GetA2ZDuration(tempDuration, matchedRateWithAssignmentTupleId));
            decimal xAmountBdt = this.PrefixMatcher.GetA2ZAmount(finalDuration, matchedRateWithAssignmentTupleId,
                rateFieldNumber: 0, //rate amount
                cdrProcessor: serviceContext.CdrProcessor);
            decimal yAmountUsd = this.PrefixMatcher.GetA2ZAmount(finalDuration, matchedRateWithAssignmentTupleId,
                rateFieldNumber: 1, //other amount1
                cdrProcessor: serviceContext.CdrProcessor);
            thisCdr.RoundedDuration = finalDuration;
            thisCdr.XAmount = xAmountBdt.RoundFractionsUpTo(serviceContext.MaxDecimalPrecision);
            thisCdr.YAmount = yAmountUsd.RoundFractionsUpTo(serviceContext.MaxDecimalPrecision);
            thisCdr.MatchedPrefixY = matchedRateWithAssignmentTupleId.Prefix;
            thisCdr.MatchedPrefixCustomer = thisCdr.MatchedPrefixY;
            if (thisCdr.CountryCode.IsNullOrEmptyOrWhiteSpace())
            {
                thisCdr.CountryCode = matchedRateWithAssignmentTupleId.CountryCode;
            }
            else if (!thisCdr.CountryCode.Equals(matchedRateWithAssignmentTupleId.CountryCode))
            {
                throw new Exception($@"Already set Country code {thisCdr.CountryCode} id different from matchedXyz rates country code {matchedRateWithAssignmentTupleId.CountryCode}");
            }
            //thisCdr.CustomerRate = matchedRateWithAssignmentTupleId.OtherAmount1;

            //add the 100ms part 
            decimal duration100 = this.PrefixMatcher.HundredMsDuration(tempDuration);
            thisCdr.Duration3 = duration100;
            DateTime callDate = thisCdr.StartTime;
            uom_conversion_dated conversionRate = GetExactOrNearestEarlierConvRateForXyz(callDate);
            if (conversionRate == null) return null;
            thisCdr.UsdRateY = Convert.ToDecimal(conversionRate.CONVERSION_FACTOR);
            decimal yBdt =
                (yAmountUsd * (decimal) thisCdr.UsdRateY).RoundFractionsUpTo(serviceContext.MaxDecimalPrecision);
            decimal zAmount = (xAmountBdt - yBdt).RoundFractionsUpTo(serviceContext.MaxDecimalPrecision);
            thisCdr.ZAmount = zAmount;
            decimal fifteenPcOfZ =
                (Convert.ToDecimal(zAmount * matchedRateWithAssignmentTupleId.OtherAmount2) / 100).RoundFractionsUpTo(
                    serviceContext.MaxDecimalPrecision);
            decimal finalAmount = 0;
            switch (xyzRatingType)
            {
                case XyzRatingType.Igw:
                    finalAmount = fifteenPcOfZ + yBdt;
                    thisCdr.RevenueIgwOut = finalAmount;
                    break;
                case XyzRatingType.Icx:
                    finalAmount = fifteenPcOfZ;
                    thisCdr.RevenueIcxOut = finalAmount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(xyzRatingType), xyzRatingType, null);
            }
            decimal btrcRevSharePercentage = Convert.ToDecimal(matchedRateWithAssignmentTupleId.OtherAmount3)
                .RoundFractionsUpTo(serviceContext.MaxDecimalPrecision);
            decimal btrcRevShareAmount =
                (fifteenPcOfZ * btrcRevSharePercentage / 100).RoundFractionsUpTo(serviceContext.MaxDecimalPrecision);
            thisCdr.Tax2 = btrcRevShareAmount;
            account postingAccount = GetPostingAccount(cdrExt, serviceContext, "BDT");
            BillingRule billingRule = GetBillingRule(serviceContext,
                matchedRateWithAssignmentTupleId.IdRatePlanAssignmentTuple);
            if (cdrExt.Cdr.ChargingStatus == 1)
            {
                var chargeable = new acc_chargeable()
                {
                    id =serviceContext.CdrProcessor.CdrJobContext.AccountingContext.AutoIncrementManager
                            .GetNewCounter(AutoIncrementCounterType.acc_chargeable),
                    uniqueBillId = thisCdr.UniqueBillId,
                    idEvent = Convert.ToInt64(thisCdr.IdCall),
                    transactionTime = callDate,
                    servicegroup = serviceContext.ServiceGroupConfiguration.IdServiceGroup,
                    assignedDirection =1, //charged to customer, although does not have assigndir 
                    servicefamily = serviceContext.ServiceFamily.Id,
                    ProductId = matchedRateWithAssignmentTupleId.ProductId,
                    idBilledUom = "BDT",
                    idQuantityUom = "TF_s", //seconds
                    BilledAmount = finalAmount,//invoiceAmount
                    Quantity = finalDuration,
                    OtherAmount1 = xAmountBdt,//xAmount
                    OtherAmount2 = yAmountUsd,//yAmount
                    OtherAmount3 = zAmount,//zAmount
                    TaxAmount1 = btrcRevShareAmount,
                    unitPriceOrCharge = matchedRateWithAssignmentTupleId.rateamount,
                    Prefix = matchedRateWithAssignmentTupleId.Prefix,
                    RateId = matchedRateWithAssignmentTupleId.id,
                    glAccountId = postingAccount.id,
                    OtherDecAmount1 = matchedRateWithAssignmentTupleId.rateamount,//xRate
                    OtherDecAmount2 = matchedRateWithAssignmentTupleId.OtherAmount1,//yRate
                    OtherDecAmount3 = thisCdr.UsdRateY,//usdRate
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

        private BillingRule GetBillingRule(ServiceContext serviceContext, int idRatePlanAssignmentTuple)
        {
            rateplanassignmenttuple idWiseRateplanAssignmenttuplesIncludingBillingRule = null;
            serviceContext.MefServiceFamilyContainer
                .IdWiseRateplanAssignmenttuplesIncludingBillingRules
                .TryGetValue(idRatePlanAssignmentTuple,out idWiseRateplanAssignmenttuplesIncludingBillingRule);
            if (idWiseRateplanAssignmenttuplesIncludingBillingRule == null)
            {
                throw new Exception($@"Billing rule not found for service family={serviceContext.ServiceFamily.ToString()}");
            }
            int idbillingRule = Convert.ToInt32(idWiseRateplanAssignmenttuplesIncludingBillingRule
                .billingruleassignment.idBillingRule);
            BillingRule billingRule = serviceContext.MefServiceFamilyContainer.BillingRules[idbillingRule];
            return billingRule;
        }

        private uom_conversion_dated GetExactOrNearestEarlierConvRateForXyz(DateTime callDate)
        {
            var year = callDate.Year;
            var month = callDate.Month - 1;
            DateTime lastMonthsUsdbBcsDateTime =
                new DateTime(year, month, DateTime.DaysInMonth(year, month), 23, 59, 59);
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
