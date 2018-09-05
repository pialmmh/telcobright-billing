using LibraryExtensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using com.google.common.@base;
using TelcobrightMediation.Accounting;
using MediationModel;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class XyzRuleHelper
    {
        public PrefixMatcher PrefixMatcher { get; set; }
        IServiceFamily Sf { get; set; }
        private UoMConvRateCache UsdBcsCache { get; }
        private XyzRatingType XyzRatingType { get; }
        public XyzRuleHelper(UoMConvRateCache usdBcsCache, PrefixMatcher prefixMatcher, IServiceFamily sf,
            XyzRatingType xyzRatingType)
        {
            this.UsdBcsCache = usdBcsCache;
            this.PrefixMatcher = prefixMatcher;
            this.Sf = sf;
            this.XyzRatingType = xyzRatingType;
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
            ServiceContext serviceContext)
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
            //else if (!thisCdr.CountryCode.Equals(matchedRateWithAssignmentTupleId.CountryCode))
            //{
            //    throw new Exception($@"Already set Country code {thisCdr.CountryCode} id different from matchedXyz rates country code {matchedRateWithAssignmentTupleId.CountryCode}");
            //}
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
            switch (this.XyzRatingType)
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
                    throw new ArgumentOutOfRangeException(nameof(this.XyzRatingType), this.XyzRatingType, null);
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

        public static InvoiceGenerationInputData ExecInvoicePreProcessing(InvoiceGenerationInputData invoiceGenerationInputData,
            XyzRatingType xyzRatingType)
        {
            int idserviceGroup = xyzRatingType == XyzRatingType.Icx ? 2 : 5;
            job telcobrightJob = invoiceGenerationInputData.TelcobrightJob;
            Dictionary<string, string> jobParamsMap = invoiceGenerationInputData.InvoiceJsonDetail;
            DateTime startDate = Convert.ToDateTime(jobParamsMap["startDate"]);
            DateTime endDate = Convert.ToDateTime(jobParamsMap["endDate"]);
            if (startDate.Day != 1 || endDate.Day != DateTime.DaysInMonth(startDate.Year, startDate.Month)
                || startDate.Hour != 0 || startDate.Minute != 0 || startDate.Second != 0 ||
                endDate.Hour != 23 || endDate.Minute != 59 || endDate.Second != 59)
            {
                throw new Exception("Start date & end date must be first & last day of a month.");
            }
            var context = invoiceGenerationInputData.Context;
            DateTime lastSecondOfPrevMonth = startDate.AddSeconds(-1);
            uom_conversion_dated usdConversionDated = context.uom_conversion_dated.Where(
                    c => c.PURPOSE_ENUM_ID == "EXTERNAL_CONVERSION"
                         && c.UOM_ID == "USD" && c.UOM_ID_TO == "BDT" && c.FROM_DATE == lastSecondOfPrevMonth).ToList()
                .FirstOrDefault();
            if (usdConversionDated == null)
                throw new Exception("Usd rate not found in uom_conversion_dated table.");
            List<acc_chargeable> sampleChargeablesForThisPeriod = context.acc_chargeable.Where(
                c => c.servicegroup == idserviceGroup && c.transactionTime >= startDate
                     && c.transactionTime < endDate).Take(20).ToList();

            if (sampleChargeablesForThisPeriod.Any(c => c.OtherDecAmount3 != usdConversionDated.CONVERSION_FACTOR))
            {
                throw new Exception(
                    $"Usd rates for outgoing calls for this period must be {usdConversionDated.CONVERSION_FACTOR}");
            }
            invoiceGenerationInputData.InvoiceJsonDetail = jobParamsMap;
            invoiceGenerationInputData.InvoiceJsonDetail.Add("usdRate",
                usdConversionDated.CONVERSION_FACTOR.ToString());
            return invoiceGenerationInputData;
        }

        public static InvoicePostProcessingData ExecInvoicePostProcessing(InvoicePostProcessingData invoicePostProcessingData)
        {
            invoice invoiceWithItem = invoicePostProcessingData.Invoice;
            invoice_item invoiceItem = invoiceWithItem.invoice_item.Single();
            Dictionary<string, string> jsonDetail = JsonConvert.DeserializeObject<Dictionary<string,string>>
                                                    (invoiceItem.JSON_DETAIL);
            InvoiceSection invoiceSection1 = GetInvoiceSection1(invoicePostProcessingData);
            jsonDetail.Add(invoiceSection1.SectionName, JsonConvert.SerializeObject(invoiceSection1));
            InvoiceSection invoiceSection2 = GetInvoiceSection2(invoicePostProcessingData);
            jsonDetail.Add(invoiceSection2.SectionName, JsonConvert.SerializeObject(invoiceSection2));
            InvoiceSection invoiceSection3 = GetInvoiceSection3(invoicePostProcessingData);
            jsonDetail.Add(invoiceSection3.SectionName, JsonConvert.SerializeObject(invoiceSection3));
            invoiceItem.JSON_DETAIL = JsonConvert.SerializeObject(jsonDetail);
            return invoicePostProcessingData;
        }
        private static InvoiceSection
            GetInvoiceSection1(InvoicePostProcessingData invoicePostProcessingData)
        {
            InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>
                invoiceSectionCreator = new InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>(
                    invoicePostProcessingData: invoicePostProcessingData, sectionNumber: 1,
                    templateName: "InternationalOutgoingToANS");
            string sql = $@"select                                                         
                       sum(successfulcalls 	)	as TotalCalls,    
                       sum(roundedduration   )/60  as TotalMinutes,   
                       sum(longDecimalAmount1)  as XAmount,
                       sum(longDecimalAmount2)  as YAmount,
                       sum(longDecimalAmount3)  as XYAmount,
                       sum(customercost      )  as Amount      
                       from sum_voice_day_02                          
                       where {invoiceSectionCreator.GetWhereClauseForDateRange()};";
            return invoiceSectionCreator.CreateInvoiceSection(sql);
        }
        private static InvoiceSection
            GetInvoiceSection2(InvoicePostProcessingData invoicePostProcessingData)
        {
            InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>
                invoiceSectionCreator = new InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>(
                    invoicePostProcessingData: invoicePostProcessingData, sectionNumber: 2,
                    templateName: "InternationalOutgoingToANSDetails1");
            string sql=$@"select p.partnername as OutPartnerName,x.TotalCalls,x.TotalMinutes,x.XAmount,
                       x.YAmount,x.XYAmount,x.Revenue from
                       (select                                                         
                       tup_outpartnerid,
                       sum(successfulcalls 	)	as TotalCalls,    
                       sum(roundedduration   )/60  as TotalMinutes,   
                       sum(longDecimalAmount1)  as XAmount,
                       sum(longDecimalAmount2)  as YAmount,
                       sum(longDecimalAmount3)  as XYAmount,
                       sum(customercost      )  as Revenue      
                       from sum_voice_day_02                          
                       where {invoiceSectionCreator.GetWhereClauseForDateRange()}
                       group by tup_outpartnerid) x                     
                       left join partner p
                       on x.tup_outpartnerid=p.idpartner;";
            return invoiceSectionCreator.CreateInvoiceSection(sql);
        }
        private static InvoiceSection
            GetInvoiceSection3(InvoicePostProcessingData invoicePostProcessingData)
        {
            InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>
                invoiceSectionCreator = new InvoiceSectionCreator<InvoiceSectionDataRowForVoiceCall>(
                    invoicePostProcessingData: invoicePostProcessingData, sectionNumber: 3,
                    templateName: "InternationalOutgoingToANSDetails2");
            string sql=$@"select                                                         
                          tup_starttime as `Date`,
                          sum(successfulcalls 	)	as TotalCalls,    
                          sum(roundedduration   )/60  as TotalMinutes,   
                          sum(longDecimalAmount1)  as XAmount,
                          sum(longDecimalAmount2)  as YAmount,
                          sum(longDecimalAmount3)  as XYAmount,
                          sum(customercost      )  as Revenue      
                          from sum_voice_day_02                          
                          where {invoiceSectionCreator.GetWhereClauseForDateRange()}
                          group by tup_starttime;";
            return invoiceSectionCreator.CreateInvoiceSection(sql);
        }
    }
}
