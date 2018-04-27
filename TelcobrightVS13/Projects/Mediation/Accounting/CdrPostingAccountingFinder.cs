using System;
using LibraryExtensions;
using MediationModel;

namespace TelcobrightMediation.Accounting
{
    public class CdrPostingAccountingFinder
    {
        private ServiceContext ServiceContext { get; }
        private Rateext RateExtWihtAssignmentTupleId { get; }
        private int IdChargedOrChargingPartner { get; }
        private BillingRule BillingRule { get; }
        private AccountingContext AccountingContext { get; }
        public CdrPostingAccountingFinder(ServiceContext serviceContext, Rateext rateExtWihtAssignmentTupleId, 
            int idChargedPartner,BillingRule billingRule)
        {
            this.ServiceContext = serviceContext;
            this.AccountingContext = serviceContext.CdrProcessor.CdrJobContext.AccountingContext;
            this.RateExtWihtAssignmentTupleId = rateExtWihtAssignmentTupleId;
            this.IdChargedOrChargingPartner = idChargedPartner;
            this.BillingRule = billingRule;
        }
        public account GetPostingAccount()
        {
            account postingAccount = null;
            //get acc based on pre or post paid                                                                           
            long idRatePlanAssignmentTuple = this.RateExtWihtAssignmentTupleId.IdRatePlanAssignmentTuple;
            
            postingAccount = GetAssignDirWiseReceivableOrPayableAccount(this.BillingRule);
            return postingAccount;
        }

        account GetAssignDirWiseReceivableOrPayableAccount(BillingRule billingRule)
        {
            account postingAccount = null;
            string idCurrencyUoM = this.ServiceContext.CdrProcessor.CdrJobContext.MediationContext.MefServiceFamilyContainer
                .DicRateplans[this.RateExtWihtAssignmentTupleId.idrateplan.ToString()].Currency;
            AccountFactory accountFactory=new AccountFactory(this.AccountingContext);
            if (this.ServiceContext.AssignDir== ServiceAssignmentDirection.Customer)
            {
                if (billingRule.IsPrepaid)//prepaid                                                                           
                {
                    postingAccount = accountFactory.CreateOrGetCustomerAccount(0, this.ServiceContext.ServiceGroupConfiguration.IdServiceGroup, this.IdChargedOrChargingPartner
                        , this.ServiceContext.ServiceFamily.Id, this.ServiceContext.ProductIdToOverrideServiceFamilyAccount, idCurrencyUoM);
                }
                else//postpaid                                                                                                
                {
                    postingAccount = accountFactory.CreateOrGetBillable(0, this.ServiceContext.ServiceGroupConfiguration.IdServiceGroup, this.IdChargedOrChargingPartner
                        , this.ServiceContext.ServiceFamily.Id, this.ServiceContext.ProductIdToOverrideServiceFamilyAccount, idCurrencyUoM);
                }
            }
            if (this.ServiceContext.AssignDir == ServiceAssignmentDirection.Supplier)
            {
                if (billingRule.IsPrepaid)//prepaid                                                                           
                {
                    postingAccount = accountFactory.CreateOrGetSupplierAccount(0, this.ServiceContext.ServiceGroupConfiguration.IdServiceGroup, this.IdChargedOrChargingPartner
                        , this.ServiceContext.ServiceFamily.Id, this.ServiceContext.ProductIdToOverrideServiceFamilyAccount, idCurrencyUoM);
                }
                else//postpaid                                                                                                
                {
                    postingAccount = accountFactory.CreateOrGetPayable(0, this.ServiceContext.ServiceGroupConfiguration.IdServiceGroup, this.IdChargedOrChargingPartner
                        , this.ServiceContext.ServiceFamily.Id, this.ServiceContext.ProductIdToOverrideServiceFamilyAccount, idCurrencyUoM);
                }
            }
            return postingAccount;
        }
        
    }
}