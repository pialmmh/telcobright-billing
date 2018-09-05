using System;
using System.ComponentModel.Composition;
using System.Globalization;
using LibraryExtensions;
using System.Collections.Generic;
using MediationModel;
using TelcobrightMediation.Accounting;
using TelcobrightMediation.Cdr;
namespace TelcobrightMediation
{

    [Export("ServiceFamily", typeof(IServiceFamily))]
    public class SfXyzIcx : IServiceFamily
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "International Outgoing [ICX]";
        public int Id => 7;
        public int GetChargedOrChargingPartnerId(CdrExt cdrExt,ServiceContext serviceContext)//thisrow,assignDir,return idPartner who's charged or charging
        {
            return Convert.ToInt32(cdrExt.Cdr.InPartnerId);
        }
        public AccChargeableExt Execute(CdrExt cdrExt,ServiceContext serviceContext, bool flagLcr)
        {
            //common for each service family ********
            int serviceFamily = this.Id;
            cdr thisCdr = cdrExt.Cdr;
            DateTime answerTime = Convert.ToDateTime(thisCdr.AnswerTime);
            int tempCategory = Convert.ToInt32(thisCdr.Category);//default 1=call
            int tempSubCategory = Convert.ToInt32(thisCdr.SubCategory);//default 1=voice
            int category = tempCategory > 0 ? tempCategory : 1;//default 1=call
            int subCategory = tempSubCategory > 0 ? tempSubCategory : 1;//default 1=voice
            //end common******************************

            string phoneNumber = thisCdr.OriginatingCalledNumber;//change per service family#####
            List<TupleByPeriod> tups = GetServiceTuple(serviceContext, answerTime);
            if (tups == null) return null;
            PrefixMatcher pr = new PrefixMatcher(serviceContext, phoneNumber, category, subCategory
                , tups, answerTime, flagLcr, useInMemoryTable: true);
            Rateext matchedRateWithIdRatePlanAssignTuple = pr.MatchPrefix();
            if (matchedRateWithIdRatePlanAssignTuple == null) return null;
            AccChargeableExt chargeableExt =
                new XyzRuleHelper(serviceContext.MefServiceFamilyContainer.UsdBcsCache, pr, this, XyzRatingType.Icx)
                .ExecuteXyzRating(matchedRateWithIdRatePlanAssignTuple, cdrExt, serviceContext);
            return chargeableExt;
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
                isBillable = 1,
                changedByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id
            };
            if (transaction.createdByJob == null)
            {
                transaction.createdByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id;
            }
            return transaction;
        }
        private List<TupleByPeriod> GetServiceTuple(ServiceContext serviceContext ,DateTime answerTime)
        {
            var x = new rateplanassignmenttuple() { idService = this.Id};
            List<rateplanassignmenttuple> match = null;
            TupleDefinitions serviceGroupWiseTupDef = null;
            serviceContext.MefServiceFamilyContainer
                .ServiceGroupWiseTupDefs.TryGetValue(serviceContext.ServiceGroupConfiguration.IdServiceGroup,
                    out serviceGroupWiseTupDef);
            if (serviceGroupWiseTupDef == null)
            {
                throw new Exception(
                    "Could not find serviceGroupWiseTupDef in " +
                    "serviceContext.MefServiceFamilyContainer.ServiceGroupWiseTupDefs");
            }
            Dictionary<string, List<rateplanassignmenttuple>> serviceTupleWithBillingRuleAssignment = 
                serviceGroupWiseTupDef
                .DicServiceTuplesIncludingBillingRuleAssignment;
            serviceTupleWithBillingRuleAssignment.TryGetValue(x.GetTuple(), out match);
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
    }
}
