using System;
using System.Collections.Generic;
using TelcobrightMediation.Accounting;
using MediationModel;
using TelcobrightMediation.Config;
namespace TelcobrightMediation
{
    public class CdrSetting : LogFileProcessorSetting
    {
        public bool EmptyFileAllowed { get; set; }
        public List<int> PartialCdrEnabledNeIds { get; set; }=new List<int>();
        public List<string> PartialCdrFlagIndicators { get; set; }
        public int SegmentSizeForDbWrite { get; set; }
        public int MaxDecimalPrecision { get; set; } = 8;
        public decimal FractionalNumberComparisonTollerance { get; set; }= .000001M;
        public int BatchSizeWhenPreparingLargeSqlJob { get; set; }
        public SummaryTimeFieldEnum SummaryTimeField { get; set; }
        public int DaysToAddBeforeAndAfterUniqueDaysForSafePartialCollection { get; set; }
        public bool DescendingOrderWhileProcessingListedFiles { get; set; }
        public new bool DescendingOrderWhileListingFiles { get; set; }
        public List<string> IllegalStrToRemoveFromFields { get; set; }
        public DateTime NotAllowedCallDateTimeBefore { get; set; } = new DateTime(2008,1,1);
        public List<IValidationRule<string[]>> ValidationRulesForInconsistentCdrs { get; set; }
        public List<IValidationRule<cdr>> ValidationRulesForCommonMediationCheck { get; set; }
        public List<int> ServiceGroupPreProcessingRules { get; set; }=new List<int>();
        public Dictionary<int, ServiceGroupConfiguration> ServiceGroupConfigurations { get; set; }
        public string NerCalculationRule { get; set; }
        public bool CallConnectTimePresent { get; set; }
        public bool DisableCdrPostProcessingJobCreationForAutomation { get; set; }
        public bool DisableParallelMediation { get; set; }
        public bool EnableTgCreationForAns { get; set; }
        public FileSplitSetting FileSplitSetting { get; set; } = null;
        public bool UseIdCallAsBillId { get; set; } = false;
        public bool AutoCorrectDuplicateBillId { get; set; } = false;
        public bool AutoCorrectDuplicateBillIdBeforeErrorProcess { get; set; } = false;
        public bool AutoCorrectBillIdsWithPrevChargeableIssue { get; set; } = false;
        public Dictionary<string, Dictionary<string, string>> ExceptionalCdrPreProcessingData { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        public int BatchSizeForCdrJobCreationCheckingExistence { get; set; } = 10000;
        public Dictionary<string, SkipSettingsForSummaryOnly> SummaryOnlySettings = new Dictionary<string, SkipSettingsForSummaryOnly>();
        public bool FilterDuplicates { get; set; }
        public CdrSetting()
        {
            this.NerCalculationRule = "NerByCauseCode";
            this.SegmentSizeForDbWrite = 30000;
            this.BatchSizeWhenPreparingLargeSqlJob = 100000;
            this.DaysToAddBeforeAndAfterUniqueDaysForSafePartialCollection = 1;
            this.IllegalStrToRemoveFromFields = new List<string>();
            this.CallConnectTimePresent = true;
            
        }
    }
}