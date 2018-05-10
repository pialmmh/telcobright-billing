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
    public class SfTollFreeEgressCharging : SfA2Z //must change class name here
    {
        public override string ToString()
        {
            return this.RuleName;
        }

        public override string RuleName => GetType().Name;
        public override string HelpText => "Toll Free Egress Charging";
        public override int Id => 5;

        protected override acc_chargeable CreateChargeable(ServiceContext serviceContext, cdr cdr, decimal finalDuration, decimal finalAmount, Rateext rateWithAssignmentTupleId, string idCurrencyUoM, BillingRule billingRule, account postingAccount)
        {
            serviceContext.AssignDir=ServiceAssignmentDirection.Customer;
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
        protected override void SetTaxAmount(ServiceContext serviceContext, cdr cdr, decimal taxAmount)
        {
            cdr.Tax1 = taxAmount;
        }

        protected override int GetChargedOrChargingPartnerId(CdrExt cdrExt,
                ServiceContext serviceContext)
        {
            return Convert.ToInt32(cdrExt.Cdr.OutPartnerId);
        }
        protected override account GetPostingAccount(ServiceContext serviceContext, Rateext rateWithAssignmentTupleId, int idChargedPartner, BillingRule billingRule)
        {
            serviceContext.AssignDir = ServiceAssignmentDirection.Customer;
            CdrPostingAccountingFinder postingAccountingFinder =
                new CdrPostingAccountingFinder(serviceContext, rateWithAssignmentTupleId, idChargedPartner,
                    billingRule);
            var postingAccount = postingAccountingFinder.GetPostingAccount();
            return postingAccount;
        }
    }
}

