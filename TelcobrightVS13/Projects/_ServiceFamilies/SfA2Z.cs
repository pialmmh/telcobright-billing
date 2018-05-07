using TelcobrightMediation;
using System.ComponentModel.Composition;
using System;
using LibraryExtensions;
using System.Linq;
using System.Collections.Generic;
using TelcobrightMediation.Accounting;
using MediationModel;
using TelcobrightMediation.Cdr;
using TransactionTuple = System.ValueTuple<int, int, long, int, long>;
namespace ServiceFamilies
{
    [Export("ServiceFamily", typeof(IServiceFamily))]
    public class SfA2Z : IServiceFamily//must change class name here
    {
        public override string ToString()
        {
            return this.RuleName;
        }
        public string RuleName => GetType().Name;
        public string HelpText => "General A2Z Prefix or Destinations based Rating";
        public int Id => 1;

        public AccChargeableExt Execute(CdrExt cdrExt, ServiceContext serviceContext, bool flagLcr)
        {
            cdr cdr = cdrExt.Cdr;
            A2ZRater a2ZRater = new A2ZRater(serviceContext, cdr);
            decimal finalDuration = 0;
            decimal finalAmount = 0;
            Rateext rateWithAssignmentTupleId = a2ZRater.ExecuteA2ZRating(out finalDuration, out finalAmount, flagLcr,
                useInMemoryTable:true);
            if (rateWithAssignmentTupleId != null)
            {
                //consider otherAmount3 as tax1, by default
                cdr.Duration1 = finalDuration;
                decimal taxAmount = Convert.ToDecimal(cdr.InPartnerCost) *
                                    Convert.ToDecimal(rateWithAssignmentTupleId.OtherAmount3) / 100;
                if (serviceContext.AssignDir == ServiceAssignmentDirection.Customer)
                {
                    cdr.Tax1 = taxAmount;
                }
                else if (serviceContext.AssignDir == ServiceAssignmentDirection.Supplier)
                {
                    cdr.Tax2 = taxAmount;
                }
                else
                {
                    throw new ArgumentException();
                }
                string idCurrencyUoM = serviceContext.CdrProcessor.CdrJobContext.MediationContext.MefServiceFamilyContainer
                    .DicRateplans[rateWithAssignmentTupleId.idrateplan.ToString()].Currency;
                int idChargedPartner = GetChargedOrChargingPartnerId(cdrExt, serviceContext);
                BillingRule billingRule = GetBillingRule(serviceContext,
                    rateWithAssignmentTupleId.IdRatePlanAssignmentTuple);
                CdrPostingAccountingFinder postingAccountingFinder =
                    new CdrPostingAccountingFinder(serviceContext, rateWithAssignmentTupleId, idChargedPartner,
                        billingRule);
                var postingAccount = postingAccountingFinder.GetPostingAccount();
                if(cdrExt.Cdr.ChargingStatus==1)
                {
                    var chargeable = new acc_chargeable
                    {
                        id = serviceContext.CdrProcessor.CdrJobContext.AccountingContext.AutoIncrementManager.GetNewCounter(AutoIncrementCounterType.acc_chargeable),
                        uniqueBillId = cdr.UniqueBillId,
                        idEvent = Convert.ToInt64(cdr.IdCall),
                        transactionTime = cdr.StartTime,
                        servicegroup = serviceContext.ServiceGroupConfiguration.IdServiceGroup,
                        assignedDirection = Convert.ToSByte(serviceContext.AssignDir),
                        servicefamily = serviceContext.ServiceFamily.Id,
                        ProductId = rateWithAssignmentTupleId.ProductId,
                        idBilledUom = idCurrencyUoM,
                        idQuantityUom = "TF_s",
                        BilledAmount = Convert.ToDecimal(finalAmount),
                        Quantity = finalDuration,
                        unitPriceOrCharge = rateWithAssignmentTupleId.rateamount,
                        Prefix = rateWithAssignmentTupleId.Prefix,
                        RateId = rateWithAssignmentTupleId.id,
                        glAccountId = postingAccount.id,
                        changedByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id,
                        idBillingrule = billingRule.Id,
                    };
                    if (serviceContext.AssignDir == ServiceAssignmentDirection.Customer)
                    {
                        chargeable.TaxAmount1 = taxAmount;
                    }
                    else if (serviceContext.AssignDir == ServiceAssignmentDirection.Supplier)
                    {
                        chargeable.TaxAmount2 = taxAmount;
                    }
                    else
                    {
                        throw new ArgumentException();
                    }

                    if (chargeable.createdByJob == null)
                    {
                        chargeable.createdByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id;
                    }

                    job telcobrightJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob;
                    chargeable.description = new List<string> {"1", "2"}.Contains(telcobrightJob.idjobdefinition.ToString())
                        ? "nc" //use new cdr flag for new & error cdr job
                        : telcobrightJob.id.ToString() == "3"? "rc" //reprocess cdr
                            : "unknown";
                    return new AccChargeableExt(chargeable)
                    {
                        RateExt = rateWithAssignmentTupleId,
                        Account = postingAccount
                    };//no rate matched
                }
            }
            return null;
        }//execute

        private BillingRule GetBillingRule(ServiceContext serviceContext,long idRatePlanAssignmentTuple)
        {
            int idbillingRule = Convert.ToInt32(serviceContext.MefServiceFamilyContainer
                .IdWiseRateplanAssignmenttuplesIncludingBillingRules[idRatePlanAssignmentTuple.ToString()]
                .billingruleassignment.idBillingRule);
            BillingRule billingRule = serviceContext.MefServiceFamilyContainer.BillingRules[idbillingRule.ToString()];
            return billingRule;
        }
        private int GetChargedOrChargingPartnerId(CdrExt cdrExt,ServiceContext serviceContext)//thisrow,assignDir,return idPartner who's charged or charging
        {
            return serviceContext.AssignDir == ServiceAssignmentDirection.Customer ? Convert.ToInt32(cdrExt.Cdr.InPartnerId) :
               serviceContext.AssignDir == ServiceAssignmentDirection.Supplier ? Convert.ToInt32(cdrExt.Cdr.OutPartnerId) : -1;
        }

        public acc_transaction GetTransaction(AccChargeableExt accChargeableExt,ServiceContext serviceContext)
        {
            acc_chargeable chargeable = accChargeableExt.AccChargeable;
            int idBillingRule = chargeable.idBillingrule;
            BillingRule billingRule= serviceContext.MefServiceFamilyContainer.BillingRules[idBillingRule.ToString()];
            acc_transaction transaction = new acc_transaction()
            {
                id = serviceContext.CdrProcessor.CdrJobContext.AutoIncrementManager
                    .GetNewCounter(AutoIncrementCounterType.acc_transaction),
                transactionTime = chargeable.transactionTime,
                debitOrCredit = "c", //single entry, use "d" for toptup, "c" for charging
                idEvent = chargeable.idEvent,
                uniqueBillId = chargeable.uniqueBillId,
                description = chargeable.description,
                glAccountId = chargeable.glAccountId,
                amount = (-1) * Convert.ToDecimal(chargeable.BilledAmount),
                uomId = chargeable.idBilledUom,
                isBillable = chargeable.assignedDirection == 1 ? Convert.ToSByte(1) : (sbyte?) null,
                isBilled = billingRule.IsPrepaid == true ? 1 : (sbyte?) null,
                changedByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id,
            };
            transaction.isPrepaid = billingRule.IsPrepaid==true?1:(sbyte?)null;
            if (transaction.createdByJob == null)
            {
                transaction.createdByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id;
            }
            return transaction;
        }
    }
}

