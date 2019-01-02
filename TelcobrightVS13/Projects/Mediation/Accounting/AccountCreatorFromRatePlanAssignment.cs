using System;
using System.Collections.Generic;
using MediationModel;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace TelcobrightMediation.Accounting
{
    public class AccountCreatorFromRatePlanAssignment : IAccountCreator
    {
        public AccountingContext AccountingContext { get; }
        public AccountFactory AccountFactory { get; }
        private PartnerEntities Context { get; }

        public AccountCreatorFromRatePlanAssignment(AccountingContext accountingContext
            , PartnerEntities context)
        {
            this.AccountingContext = accountingContext;
            AccountFactory = new AccountFactory(this.AccountingContext);
            this.Context = context;
        }

        public void CreateAllMissingCustomerAccountsFromRatePlanAssignmentInfo()
        {
            List<rateplanassignmenttuple> rTuples =
                Context.rateplanassignmenttuples
                    .Where(rp => rp.AssignDirection == 1 && rp.idpartner > 0)
                    .Include(rt => rt.billingruleassignment.jsonbillingrule)
                    .Include(rt => rt.rateassigns)
                    .Where(rp => rp.rateassigns.Any()).ToList();//tuple may exist without assigned rateplan

            foreach (rateplanassignmenttuple tup in rTuples)
            {
                int productId = 0; //if req find use cases for this later
                int depth = 0; //if req find use cases for this later
                int idPartner = Convert.ToInt32(tup.idpartner);
                if (idPartner <= 0)
                    throw new Exception("Partner id must be >=0;");
                BillingRule billingRule =
                    JsonBillingRuleToBillingRuleConverter.Convert(tup.billingruleassignment.jsonbillingrule);
                if (billingRule == null)
                    throw new Exception($"Billing rule not found for idRatePlanAssignmentTuple={tup.id}");
                int idServiceGroup = tup.billingruleassignment.idServiceGroup;
                if (idServiceGroup <= 0)
                    throw new Exception("Service group id must be >=0;");
                int idServiceFamily = tup.idService;
                if (idServiceFamily <= 0)
                    throw new Exception("Service family id must be >=0;");
                string uom = tup.rateassigns.First().rateplan.Currency;
                if (string.IsNullOrEmpty(uom))
                    throw new Exception("Found currency is empty in rateplan, which is not supported " +
                                        "and possibly erroneous.");
                switch (billingRule.IsPrepaid)
                {
                    case true:
                        this.AccountFactory.CreateOrGetCustomerBilled(depth, idServiceGroup, idPartner, idServiceFamily,
                            productId, uom);
                        break;
                    default:
                        this.AccountFactory.CreateOrGetBillable(depth, idServiceGroup, idPartner, idServiceFamily,
                            productId, uom);
                        break;
                }
            }
            this.AccountingContext.WriteAllChanges();
        }
    }
}