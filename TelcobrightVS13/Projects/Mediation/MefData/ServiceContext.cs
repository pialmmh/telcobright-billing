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
        public int IdServiceGroup { get; set; }
        public IServiceFamily ServiceFamily { get; set; }
        public int ProductIdToOverrideServiceFamilyAccount { get; set; }
        public ServiceAssignmentDirection AssignDir { get; set; }

        public ServiceContext(CdrProcessor cdrProcessor,
            IServiceGroup serviceGroup, IServiceFamily serviceFamily,
            ServiceAssignmentDirection assignDir, int productIdToOverrideServiceFamilyAccount)
        {
            this.CdrProcessor = cdrProcessor;
            this.MefServiceFamilyContainer = this.CdrProcessor.CdrJobContext.MediationContext.MefServiceFamilyContainer;
            this.IdServiceGroup = serviceGroup.Id;
            this.ServiceFamily = serviceFamily;
            this.ProductIdToOverrideServiceFamilyAccount = productIdToOverrideServiceFamilyAccount;
            this.AssignDir = assignDir;
        }
    }
}