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
        public CdrSetting CdrSetting { get; }
        public int MaxDecimalPrecision => this.CdrSetting.MaxDecimalPrecision;
        public DigitRulesData DigitRulesData { get; }
        public ServiceContext(CdrProcessor cdrProcessor,
            ServiceGroupConfiguration serviceGroupConfiguration, IServiceFamily serviceFamily,
            ServiceAssignmentDirection assignDir, int productIdToOverrideServiceFamilyAccount,
            DigitRulesData digitRulesData)
        {
            this.CdrProcessor = cdrProcessor;
            this.MefServiceFamilyContainer = this.CdrProcessor.CdrJobContext.MediationContext.MefServiceFamilyContainer;
            this.CdrSetting = this.CdrProcessor.CdrJobContext.MediationContext.CdrSetting;
            this.ServiceGroupConfiguration = serviceGroupConfiguration;
            this.ServiceFamily = serviceFamily;
            this.ProductIdToOverrideServiceFamilyAccount = productIdToOverrideServiceFamilyAccount;
            this.AssignDir = assignDir;
            this.DigitRulesData = digitRulesData;
        }
    }
}