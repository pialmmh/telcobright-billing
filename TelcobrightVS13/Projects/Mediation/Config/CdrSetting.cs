using System;
using System.Collections.Generic;
using LibraryExtensions;
using TelcobrightMediation.Accounting;
using MediationModel;
using TelcobrightMediation.Config;
namespace TelcobrightMediation
{
    public class CdrSetting : LogFileProcessorSetting
    {
        public bool EmptyFileAllowed { get; set; }
        public List<int> PartialCdrEnabledNeIds { get; set; } = new List<int>();
        public List<string> PartialCdrFlagIndicators { get; set; }
        public int SegmentSizeForDbWrite { get; set; }
        public int MaxDecimalPrecision { get; set; } = 8;
        public decimal FractionalNumberComparisonTollerance { get; set; } = .000001M;
        public int BatchSizeWhenPreparingLargeSqlJob { get; set; }
        public SummaryTimeFieldEnum SummaryTimeField { get; set; }
        public int DaysToAddBeforeAndAfterUniqueDaysForSafePartialCollection { get; set; } = 1;
        public int SecondsBeforeForOldEventScan { get; set; } = 300;//this will be used if >0
        public int SecondsAfterForOldEventScan { get; set; } = 300;//this will be used if >0
        public int HoursToAddBeforeForSafePartialCollection { get; set; } = 1;
        public int HoursToAddAfterForSafePartialCollection { get; set; } = 1;
        public bool DescendingOrderWhileProcessingListedFiles { get; set; }
        public new bool DescendingOrderWhileListingFiles { get; set; }
        public new bool DescendingOrderWhileListingFilesByFileNameOnly { get; set; }
        public new int FileNameLengthFromRightWhileSorting{ get; set; }
        public List<string> IllegalStrToRemoveFromFields { get; set; }
        public DateTime NotAllowedCallDateTimeBefore { get; set; } = new DateTime(2008, 1, 1);
        public List<IValidationRule<string[]>> ValidationRulesForInconsistentCdrs { get; set; }
        public List<IValidationRule<cdr>> ValidationRulesForCommonMediationCheck { get; set; }
        public List<int> ServiceGroupPreProcessingRules { get; set; } = new List<int>();
        public Dictionary<int, ServiceGroupConfiguration> ServiceGroupConfigurations { get; set; }
        public string NerCalculationRule { get; set; }
        public bool CallConnectTimePresent { get; set; }
        public bool DisableCdrPostProcessingJobCreationForAutomation { get; set; }
        public bool DisableParallelMediation { get; set; }
        public bool EnableTgCreationForAns { get; set; }
        public bool AutoCorrectDuplicateBillId { get; set; } = false;
        //public bool IgnoreDuplicatesAfterDuplicateFiltering { get; set; } = false;
        public bool AutoCorrectDuplicateBillIdBeforeErrorProcess { get; set; } = false;
        public bool AutoCorrectBillIdsWithPrevChargeableIssue { get; set; } = false;
        public Dictionary<string, Dictionary<string, string>> ExceptionalCdrPreProcessingData { get; set; } = new Dictionary<string, Dictionary<string, string>>();
        public int BatchSizeForCdrJobCreationCheckingExistence { get; set; } = 10000;
        public SkipSettingsForSummaryOnly SkipSettingsForSummaryOnly = new SkipSettingsForSummaryOnly();
        public FileSplitSetting FileSplitSetting { get; set; }
        public bool useCasStyleProcessing { get; set; } = false;
        public bool useSmsHubProcessing { get; set; } = false;
        public Dictionary<int, NeAdditionalSetting> NeWiseAdditionalSettings { get; set; } = new Dictionary<int, NeAdditionalSetting>();
        public bool UnzipCompressedFiles { get; set; }
        public bool DeleteOriginalArchiveAfterUnzip { get; set; }
        public DateRange SameRatePeriodForICX { get; set; } = new DateRange(new DateTime(2017, 01, 01), new DateTime(2030, 01, 01));
        public bool EnableSameRatePeriodForICX { get; set; } = true;
        public bool ProcessNewCdrJobsBeforeReProcess { get; set; }
        public bool WriteFailedCallsToDb { get; set; }
        public bool WriteCdrDiscarded { get; set; }
        public DateTime ExcludeBefore { get; set; } 
        public bool isTableDelete { get; set; }
        public bool AllowNegativeInvoiceGeneration { get; set; } = false;
        public string MoveCdrToDriveAfterProcessing { get; set; }
        public ExeRestartType WatchDogRestartRule { get; set; } = ExeRestartType.None;//this will be used if >0
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