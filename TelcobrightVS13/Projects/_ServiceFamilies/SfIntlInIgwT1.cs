using System;
using System.ComponentModel.Composition;
using System.Globalization;
using LibraryExtensions;
using System.Collections.Generic;
using TelcobrightMediation.Accounting;
using System.Linq;
using MediationModel;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{

    [Export("ServiceFamily", typeof(IServiceFamily))]
    public class SfIntlInIgwT1 : IServiceFamily
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "International Incoming Rate Through IOF [T1 IGW via IOS]";
        public int Id => 3;
        public int GetChargedOrChargingPartnerId(CdrExt newCdrExt, ServiceContext serviceContext)//thisrow,assignDir,return idPartner who's charged or charging
        {
            return Convert.ToInt32(newCdrExt.Cdr.OutPartnerId);
        }

        public AccChargeableExt Execute(CdrExt cdrExt, ServiceContext serviceContext, bool flagLcr)
        {
            //common for each service family ********
            cdr thisCdr = cdrExt.Cdr;
            int serviceFamily = this.Id;
            PrefixMatcher pr = new PrefixMatcher(serviceContext);
            DateTime answerTime;
            if (thisCdr.ChargingStatus == 1)
            {
                answerTime = Convert.ToDateTime(thisCdr.AnswerTime);
            }
            else //failed call, use start time as answertime, for prefix matching, duration and amount will be 0
            {
                answerTime = thisCdr.StartTime;
            }
            int tempCategory = Convert.ToInt32(thisCdr.Category); //default 1=call
            int tempSubCategory = Convert.ToInt32(thisCdr.SubCategory);
            
            int category = tempCategory > 0 ? tempCategory : 1; //default 1=call
            int subCategory = tempSubCategory > 0 ? tempSubCategory : 1; //default 1=voice
            
            string phoneNumber = thisCdr.TerminatingCalledNumber; //change per service family#####
            if (phoneNumber.Trim() == "") return null; //term no can be empty e.g. for failed calls

            List<TupleByPeriod> tups = GetServiceTuple(serviceContext, answerTime);
            if (tups == null) return null;
            int maxDecimalPrecision = serviceContext.MaxDecimalPrecision;
            Rateext matchedRateWithAssignmentTupleId = pr.MatchPrefix(phoneNumber, category, subCategory, tups,
                answerTime, flagLcr,useInMemoryTable:true);
            matchedRateWithAssignmentTupleId.rateamount =
                matchedRateWithAssignmentTupleId.rateamount.RoundFractionsUpTo(maxDecimalPrecision);
            if (matchedRateWithAssignmentTupleId == null) return null;

            //iof over selling rule
            //Minute rate    other amt   Effect Rate IOF % BTRC % IOF First Amount    IOF Additional Amount IOF Total Amount    BTRC Amount
            //1   0.01500000000   0.00000000000   0.01500000000   0.60000000000   0.40000000000   0.00900000000   0.00000000000   0.00900000000   0.00600000000
            //1   0.02000000000   0.00500000000   0.01500000000   0.60000000000   0.40000000000   0.00900000000   0.00500000000   0.01400000000   0.00600000000

            long finalDuration = 0;
            finalDuration = Convert.ToInt64(pr.GetA2ZDuration(thisCdr.DurationSec,matchedRateWithAssignmentTupleId));
            //effective termination rate
            decimal additionalChargeIof = Convert.ToDecimal(matchedRateWithAssignmentTupleId.OtherAmount1)
                .RoundFractionsUpTo(maxDecimalPrecision); //.005c
            Rateext tempRate = new Rateext()
            {
                rateamount = (matchedRateWithAssignmentTupleId.rateamount - additionalChargeIof) //2c becomes 1.5c
            };
            decimal terminatingAmount = 0;
            terminatingAmount =
                (finalDuration * tempRate.rateamount / 60).RoundFractionsUpTo(maxDecimalPrecision); //becomes 1.5c excluding .005 for 1 minute

            decimal additionalAmountIof =
                (finalDuration * additionalChargeIof / 60).RoundFractionsUpTo(maxDecimalPrecision);

            decimal iosPercentage = Convert.ToDecimal(matchedRateWithAssignmentTupleId.OtherAmount2)
                .RoundFractionsUpTo(maxDecimalPrecision);
            decimal btrcPercentage = Convert.ToDecimal(matchedRateWithAssignmentTupleId.OtherAmount3)
                .RoundFractionsUpTo(maxDecimalPrecision);

            decimal iosFirstAmount = 0;
            decimal btrcAmount = 0;
            decimal iosTotalAmount = 0;

            iosFirstAmount = (terminatingAmount * iosPercentage).RoundFractionsUpTo(maxDecimalPrecision);
            btrcAmount = (terminatingAmount * btrcPercentage).RoundFractionsUpTo(maxDecimalPrecision);
            iosTotalAmount = (iosFirstAmount + additionalAmountIof).RoundFractionsUpTo(maxDecimalPrecision);

            thisCdr.Tax1 = btrcAmount.RoundFractionsUpTo(maxDecimalPrecision);
            thisCdr.CostIcxIn = Convert.ToDecimal(iosTotalAmount).RoundFractionsUpTo(maxDecimalPrecision);
            thisCdr.CountryCode = matchedRateWithAssignmentTupleId.CountryCode;
            thisCdr.MatchedPrefixSupplier = matchedRateWithAssignmentTupleId.Prefix;
            thisCdr.RoundedDuration = finalDuration;
            thisCdr.SupplierRate = matchedRateWithAssignmentTupleId.rateamount;
            int idChargedPartner = GetChargedOrChargingPartnerId(cdrExt, serviceContext);
            BillingRule billingRule = GetBillingRule(serviceContext,
                matchedRateWithAssignmentTupleId.IdRatePlanAssignmentTuple);
            CdrPostingAccountingFinder postingAccountingFinder =
                new CdrPostingAccountingFinder(serviceContext, matchedRateWithAssignmentTupleId, idChargedPartner,
                    billingRule);
            //charging information for supplier direction by ios, but this service family is not really assigned to
            //any ios, thereby change the service context here to "supplier"
            serviceContext.AssignDir = ServiceAssignmentDirection.Supplier;
            account postingAccount = postingAccountingFinder.GetPostingAccount();
            rateplan dicRateplan = null;
            serviceContext.CdrProcessor.CdrJobContext.MediationContext.MefServiceFamilyContainer
                .DicRateplans.TryGetValue((long)matchedRateWithAssignmentTupleId.idrateplan, out dicRateplan);
            if (dicRateplan==null)
            {
                throw new Exception("Could not find rateplan.");
            }
            string idCurrencyUoM = dicRateplan.Currency;
            if(cdrExt.Cdr.ChargingStatus==1)
            {
                acc_chargeable chargeable = new acc_chargeable()
                {
                    id = serviceContext.CdrProcessor.CdrJobContext.AccountingContext.AutoIncrementManager
                        .GetNewCounter(AutoIncrementCounterType.acc_chargeable),
                    uniqueBillId = thisCdr.UniqueBillId,
                    idEvent = Convert.ToInt64(thisCdr.IdCall),
                    transactionTime = thisCdr.StartTime,
                    servicegroup = serviceContext.ServiceGroupConfiguration.IdServiceGroup,
                    assignedDirection = 2, //indicates a bill by supplier (IOS/ICX)
                    servicefamily = serviceContext.ServiceFamily.Id,
                    ProductId = matchedRateWithAssignmentTupleId.ProductId,
                    idBilledUom = idCurrencyUoM,
                    idQuantityUom = "TF_s",
                    BilledAmount = Convert.ToDecimal(iosTotalAmount),
                    Quantity = finalDuration,
                    TaxAmount1 = btrcAmount,
                    unitPriceOrCharge = matchedRateWithAssignmentTupleId.rateamount,
                    Prefix = matchedRateWithAssignmentTupleId.Prefix,
                    RateId = matchedRateWithAssignmentTupleId.id,
                    glAccountId = postingAccount.id,
                    changedByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id,
                    idBillingrule = billingRule.Id,
                    description =
                        new List<string> {"1", "2"}.Contains(serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob
                            .idjobdefinition.ToString())
                            ? "nc" //use new cdr flag for new & error cdr job
                            : serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.idjobdefinition.ToString() == "3"
                                ? "rc" //reprocess cdr
                                : "unknown",
                };
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

        private List<TupleByPeriod> GetServiceTuple(ServiceContext serviceContext, DateTime answerTime)
        {
            var x = new rateplanassignmenttuple() { idService = this.Id };

            List<rateplanassignmenttuple> match = null;
            TupleDefinitions serviceGroupWiseTupDef = null;
            serviceContext.MefServiceFamilyContainer
                .ServiceGroupWiseTupDefs.TryGetValue(serviceContext.ServiceGroupConfiguration.IdServiceGroup,
                    out serviceGroupWiseTupDef);
            if (serviceGroupWiseTupDef==null)
            {
                throw new Exception(
                    "Could not find serviceGroupWiseTupDef in " +
                    "serviceContext.MefServiceFamilyContainer.ServiceGroupWiseTupDefs");
            }
            serviceGroupWiseTupDef
                .DicServiceTuplesIncludingBillingRuleAssignment.TryGetValue(x.GetTuple(), out match);

            if (match != null)
            {
                DateRange dRange = new DateRange();
                dRange.StartDate = answerTime.Date;
                dRange.EndDate = answerTime.Date.AddDays(1);
                List<TupleByPeriod> lstTupleByPeriods = new List<TupleByPeriod>();
                foreach (rateplanassignmenttuple rtup in match)
                {
                    lstTupleByPeriods.Add(new TupleByPeriod()
                    {
                        DRange = dRange,
                        IdAssignmentTuple = rtup.id,
                        Priority = rtup.priority
                    });
                }
                return lstTupleByPeriods;
            }
            else return null;
        }
        public acc_transaction GetTransaction(AccChargeableExt accChargeableExt, ServiceContext serviceContext)
        {
            acc_chargeable chargeable = accChargeableExt.AccChargeable;
            acc_transaction transaction = new acc_transaction()
            {
                id = serviceContext.CdrProcessor.CdrJobContext.AutoIncrementManager.GetNewCounter(AutoIncrementCounterType.acc_transaction),
                transactionTime = chargeable.transactionTime,
                debitOrCredit = "c", //single entry, use "d" for toptup, "c" for charging
                idEvent = chargeable.idEvent,
                uniqueBillId = chargeable.uniqueBillId,
                description = chargeable.description,
                glAccountId = chargeable.glAccountId,
                amount = (-1) * Convert.ToDecimal(chargeable.BilledAmount),
                uomId = chargeable.idBilledUom,
                changedByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id,
             };
            if (transaction.createdByJob == null)
            {
                transaction.createdByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id;
            }
            return transaction;
        }
    }
}
