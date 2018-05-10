using System;
using LibraryExtensions;
using TelcobrightMediation.MefData.GenericAssignment;
using TransactionTuple = System.ValueTuple<int, int, long, int,long>;
using TelcobrightMediation.Cdr;

namespace TelcobrightMediation
{
    public class ServiceContext
    {
        public CdrProcessor CdrProcessor { get; }
        public MefServiceFamilyContainer MefServiceFamilyContainer { get; }
        public ServiceGroupConfiguration ServiceGroupConfiguration { get; }
        public IServiceFamily ServiceFamily { get; set; }
        public int ProductIdToOverrideServiceFamilyAccount { get;}
        public ServiceAssignmentDirection AssignDir { get; set; }
        public ServiceContext(CdrProcessor cdrProcessor,
            ServiceGroupConfiguration serviceGroupConfiguration, IServiceFamily serviceFamily,
            ServiceAssignmentDirection assignDir, int productIdToOverrideServiceFamilyAccount)
        {
            this.CdrProcessor = cdrProcessor;
            this.MefServiceFamilyContainer = this.CdrProcessor.CdrJobContext.MediationContext.MefServiceFamilyContainer;
            this.ServiceGroupConfiguration = serviceGroupConfiguration;
            this.ServiceFamily = serviceFamily;
            this.ProductIdToOverrideServiceFamilyAccount = productIdToOverrideServiceFamilyAccount;
            this.AssignDir = assignDir;
        }
    }
}