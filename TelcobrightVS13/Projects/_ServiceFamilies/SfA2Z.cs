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
        public virtual  string RuleName => GetType().Name;
        public virtual string HelpText => "General A2Z Prefix or Destinations based Rating";
        public virtual int Id => 1;

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
                rateplan dicRateplan = null;
                serviceContext.CdrProcessor.CdrJobContext.MediationContext.MefServiceFamilyContainer
                    .DicRateplans.TryGetValue((int)rateWithAssignmentTupleId.idrateplan,out dicRateplan);
                if (dicRateplan == null) throw new Exception("Could not find rateplan.");
                string idCurrencyUoM = dicRateplan.Currency;
                int idChargedPartner = GetChargedOrChargingPartnerId(cdrExt, serviceContext);
                BillingRule billingRule = GetBillingRule(serviceContext,
                    rateWithAssignmentTupleId.IdRatePlanAssignmentTuple);
                account postingAccount = GetPostingAccount(serviceContext, rateWithAssignmentTupleId, idChargedPartner, billingRule);
                if (cdrExt.Cdr.ChargingStatus == 1)
                {
                    decimal taxAmount = 0;
                    SetTaxAmount(serviceContext, cdr, rateWithAssignmentTupleId, out taxAmount);
                    acc_chargeable chargeable = CreateChargeable(serviceContext, cdr, finalDuration, finalAmount,
                        rateWithAssignmentTupleId, idCurrencyUoM, billingRule, postingAccount);
                    if (chargeable.createdByJob == null)
                    {
                        chargeable.createdByJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob.id;
                    }

                    job telcobrightJob = serviceContext.CdrProcessor.CdrJobContext.TelcobrightJob;
                    chargeable.description = new List<string> { "1", "2" }.Contains(telcobrightJob.idjobdefinition.ToString())
                        ? "nc" //use new cdr flag for new & error cdr job
                        : telcobrightJob.id.ToString() == "3" ? "rc" //reprocess cdr
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

        protected virtual acc_chargeable CreateChargeable(ServiceContext serviceContext, cdr cdr, decimal finalDuration, decimal finalAmount, Rateext rateWithAssignmentTupleId, string idCurrencyUoM, BillingRule billingRule, account postingAccount)
        {
            return new acc_chargeable
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
        }

        protected virtual account GetPostingAccount(ServiceContext serviceContext, Rateext rateWithAssignmentTupleId, int idChargedPartner, BillingRule billingRule)
        {
            CdrPostingAccountingFinder postingAccountingFinder =
                                new CdrPostingAccountingFinder(serviceContext, rateWithAssignmentTupleId, idChargedPartner,
                                    billingRule);
            var postingAccount = postingAccountingFinder.GetPostingAccount();
            return postingAccount;
        }

        protected virtual void SetTaxAmount(ServiceContext serviceContext, cdr cdr,
            Rateext rateWithAssignmentTupleId, out decimal taxAmount)
        {
            taxAmount = Convert.ToDecimal(cdr.InPartnerCost) *
                                Convert.ToDecimal(rateWithAssignmentTupleId.OtherAmount3) / 100;
            if (taxAmount>0)
            {
                taxAmount = decimal.Round(taxAmount, serviceContext.MaxDecimalPrecision);
            }
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
        }

        protected virtual int GetChargedOrChargingPartnerId(CdrExt cdrExt, ServiceContext serviceContext)//thisrow,assignDir,return idPartner who's charged or charging
        {
            return serviceContext.AssignDir == ServiceAssignmentDirection.Customer ? Convert.ToInt32(cdrExt.Cdr.InPartnerId) :
                serviceContext.AssignDir == ServiceAssignmentDirection.Supplier ? Convert.ToInt32(cdrExt.Cdr.OutPartnerId) : -1;
        }

        private BillingRule GetBillingRule(ServiceContext serviceContext,int idRatePlanAssignmentTuple)
        {
            rateplanassignmenttuple idWiseRateplanAssignmenttuplesIncludingBillingRule = null;
            serviceContext.MefServiceFamilyContainer
                .IdWiseRateplanAssignmenttuplesIncludingBillingRules
                .TryGetValue(idRatePlanAssignmentTuple,out idWiseRateplanAssignmenttuplesIncludingBillingRule);
            if (idWiseRateplanAssignmenttuplesIncludingBillingRule==null)
            {
                throw new Exception($@"Billing rule not found for service family={serviceContext.ServiceFamily.ToString()}");
            }
            int idbillingRule = Convert.ToInt32(idWiseRateplanAssignmenttuplesIncludingBillingRule
                .billingruleassignment.idBillingRule);
            BillingRule billingRule = serviceContext.MefServiceFamilyContainer.BillingRules[idbillingRule];
            return billingRule;
        }
        
        public acc_transaction GetTransaction(AccChargeableExt accChargeableExt,ServiceContext serviceContext)
        {
            acc_chargeable chargeable = accChargeableExt.AccChargeable;
            int idBillingRule = chargeable.idBillingrule;
            BillingRule billingRule= serviceContext.MefServiceFamilyContainer.BillingRules[idBillingRule];
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

