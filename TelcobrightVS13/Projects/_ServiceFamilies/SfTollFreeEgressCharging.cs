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

        protected override void GetTaxAmount(ServiceContext serviceContext, cdr cdr, decimal taxAmount)
        {
            if (serviceContext.AssignDir == ServiceAssignmentDirection.Customer)
            {
                throw new Exception("Rateplan must be assigned in supplier direction for TollFree Egress charging calls.");
            }
            if (serviceContext.AssignDir == ServiceAssignmentDirection.Supplier)
            {
                cdr.Tax1= taxAmount;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        protected override int GetChargedOrChargingPartnerId(CdrExt cdrExt,
                ServiceContext serviceContext)
        {
            return serviceContext.AssignDir == ServiceAssignmentDirection.Supplier
                ? Convert.ToInt32(cdrExt.Cdr.OutPartnerId): -1;
        }
    }
}

