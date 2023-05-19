using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;

namespace TelcobrightMediation.Config
{
    public static class DummySwitch
    {
        public static ne getDummyNe()
        {
            return new ne
            {
                idSwitch = 0,
                idCustomer = 0,
                idcdrformat = 2,
                idMediationRule = 1,
                SwitchName = "dummy",
                CDRPrefix = null,
                FileExtension = null,
                Description = null,
                SourceFileLocations = null,
                BackupFileLocations = null,
                LoadingStopFlag = null,
                LoadingSpanCount = null,
                TransactionSizeForCDRLoading = null,
                DecodingSpanCount = null,
                SkipAutoCreateJob = 1,
                SkipCdrListed = 1,
                SkipCdrReceived = 1,
                SkipCdrDecoded = 1,
                SkipCdrBackedup = 1,
                KeepDecodedCDR = 1,
                KeepReceivedCdrServer = 1,
                CcrCauseCodeField = null,
                SwitchTimeZoneId = null,
                CallConnectIndicator = "CT",
                FieldNoForTimeSummary = 29,
                EnableSummaryGeneration = "0",
                ExistingSummaryCacheSpanHr = 6,
                BatchToDecodeRatio = 3,
                PrependLocationNumberToFileName = 0
            };
        }
    }
}
