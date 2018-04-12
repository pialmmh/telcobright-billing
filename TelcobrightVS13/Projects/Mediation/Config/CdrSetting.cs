using System;
using System.Collections.Generic;
using TelcobrightMediation.Accounting;

namespace TelcobrightMediation
{
    public class CdrSetting : LogFileProcessorSetting
    {
        public bool ConsiderEmptyCdrFilesAsValid { get; set; }
        public List<int> PartialCdrEnabledNeIds { get; set; }=new List<int>();
        public int SegmentSizeForDbWrite { get; set; }
        public int NoOfDecimalPrecisionDigitsForDoubles { get; set; }
        public int BatchSizeWhenPreparingLargeSqlJob { get; set; }
        public SummaryTimeFieldEnum SummaryTimeField { get; set; }
        public int DaysToAddBeforeAndAfterUniqueDaysForSafePartialCollection { get; set; }
        public bool DescendingOrderWhileProcessingListedFiles { get; set; }
        public new bool DescendingOrderWhileListingFiles { get; set; }
        public List<string> IllegalStrToRemoveFromFields { get; set; }
        public DateTime NotAllowedCallDateTimeBefore { get; set; } = new DateTime(2008,1,1);
        public Dictionary<string, string> ValidationRulesForInconsistentCdrs { get; set; }
        public Dictionary<string,string> CommonMediationChecklist { get; set; }
        public Dictionary<int, ServiceGroupConfiguration> ServiceGroupConfigurations { get; set; }
        public string NerCalculationRule { get; set; }
        public bool CallConnectTimePresent { get; set; }
        public CdrSetting()
        {
            this.NerCalculationRule = "NerByCauseCode";
            this.SegmentSizeForDbWrite = 10000;
            this.BatchSizeWhenPreparingLargeSqlJob = 1000000; //1M
            this.DaysToAddBeforeAndAfterUniqueDaysForSafePartialCollection = 1;
            this.IllegalStrToRemoveFromFields = new List<string>();
            this.NoOfDecimalPrecisionDigitsForDoubles = 10;
            this.CallConnectTimePresent = true;
        }

    }

}