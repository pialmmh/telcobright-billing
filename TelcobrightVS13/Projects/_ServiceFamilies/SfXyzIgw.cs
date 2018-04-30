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
    public class SfXyzIgw : IServiceFamily
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "International Outgoing Rev Share [IGW-Old]";
        public int Id => 4;
        public int GetChargedOrChargingPartnerId(CdrExt newCdrExt,ServiceContext serviceContext)//thisrow,assignDir,return idPartner who's charged or charging
        {
            return Convert.ToInt32(newCdrExt.Cdr.InPartnerId);
        }
        public AccChargeableExt Execute(CdrExt cdrExt,ServiceContext serviceContext, bool flagLcr)
        {
            //common for each service family ********
            cdr thisCdr = cdrExt.Cdr;
            int serviceFamily = this.Id;
            PrefixMatcher pr = new PrefixMatcher(serviceContext);
            DateTime answerTime = new DateTime();
            if (thisCdr.ChargingStatus == 1)
            {
                answerTime = Convert.ToDateTime(thisCdr.AnswerTime);
            }
            else //failed call, use start time as answertime, for prefix matching, duration and amount will be 0
            {
                answerTime = thisCdr.StartTime;
            }
            int tempCategory = thisCdr.Category.ConvertToNonNullableValueOrZeroIfNull();//default 1=call
            int tempSubCategory = thisCdr.SubCategory.ConvertToNonNullableValueOrZeroIfNull();//default 1=voice
            
            int category = tempCategory > 0 ? tempCategory : 1;//default 1=call
            int subCategory = tempSubCategory > 0 ? tempSubCategory : 1;//default 1=voice
            //end common******************************
            string phoneNumber = thisCdr.OriginatingCalledNumber;//change per service family#####
            
            List<TupleByPeriod> tups = GetServiceTuple(serviceContext, answerTime);
            if (tups == null) return null;
            Rateext matchedRateWithIdRatePlanAssignTuple = pr.MatchPrefix(phoneNumber, category, subCategory, tups,
                                                            answerTime, flagLcr,useInMemoryTable:true);
            if (matchedRateWithIdRatePlanAssignTuple == null) return null;
            AccChargeableExt chargeableExt =
                new XyzRuleHelper(serviceContext.MefServiceFamilyContainer.UsdBcsCache, pr, this).ExecuteXyzRating(
                    matchedRateWithIdRatePlanAssignTuple, cdrExt, serviceContext, XyzRatingType.Igw);
            return chargeableExt;
        }
        private List<TupleByPeriod> GetServiceTuple(ServiceContext serviceContext, DateTime answerTime)
        {
            var x = new rateplanassignmenttuple() { idService = this.Id };
            List<rateplanassignmenttuple> match = null;
            serviceContext.MefServiceFamilyContainer
                .ServiceGroupWiseTupDefs[serviceContext.ServiceGroupConfiguration.IdServiceGroup.ToString()]
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
                id = serviceContext.CdrProcessor.CdrJobContext.AutoIncrementManager.GetNewCounter("acc_transaction"),
                transactionTime = chargeable.transactionTime,
                debitOrCredit = "c", //single entry, use "d" for toptup, "c" for charging
                idEvent = chargeable.idEvent,
                uniqueBillId = chargeable.uniqueBillId,
                description = chargeable.description,
                glAccountId = chargeable.glAccountId,
                amount = (-1) * Convert.ToDecimal(chargeable.BilledAmount),
                uomId = chargeable.idBilledUom,
                isBillable = 1,
                createdByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id
            };
            return transaction;
        }
    }
}
