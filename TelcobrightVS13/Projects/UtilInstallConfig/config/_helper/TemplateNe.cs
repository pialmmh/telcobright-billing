using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediationModel;
namespace InstallConfig.config._helper
{
    class TemplateNeFactory
    {
        public static ne GetInstanceNe()
        {
            return new ne
            {
                idSwitch = -1,
                idCustomer = -1,
                idcdrformat = -17,
                idMediationRule = 2,
                SwitchName = "none",
                CDRPrefix = "none",
                FileExtension = ".DAT",
                Description = null,
                SourceFileLocations = "none",
                BackupFileLocations = null,
                LoadingStopFlag = null,
                LoadingSpanCount = 100,
                TransactionSizeForCDRLoading = 1500,
                DecodingSpanCount = 100,
                SkipAutoCreateJob = 1,
                SkipCdrListed = 0,
                SkipCdrReceived = 1,
                SkipCdrDecoded = 0,
                SkipCdrBackedup = 1,
                KeepDecodedCDR = 1,
                KeepReceivedCdrServer = 1,
                CcrCauseCodeField = 56,
                SwitchTimeZoneId = null,
                CallConnectIndicator = "F5",
                FieldNoForTimeSummary = 29,
                EnableSummaryGeneration = "1",
                ExistingSummaryCacheSpanHr = 6,
                BatchToDecodeRatio = 3,
                PrependLocationNumberToFileName = 0,
                UseIdCallAsBillId = 1,
                
            };
        }
    }
}
