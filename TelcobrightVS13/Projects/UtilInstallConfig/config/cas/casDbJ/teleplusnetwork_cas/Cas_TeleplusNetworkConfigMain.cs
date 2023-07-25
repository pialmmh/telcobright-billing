using Newtonsoft.Json;
using System;
using System.IO;
using TelcobrightFileOperations;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightMediation;
using TelcobrightMediation.Config;
using FlexValidation;
using InstallConfig._CommonValidation;
using InstallConfig._generator;
using MediationModel;
using TelcobrightInfra;
using TelcobrightMediation.Accounting;

namespace InstallConfig
{
    [Export(typeof(AbstractConfigGenerator))]
    public partial class CasTeleplusNetworkAbstractConfigGenerator : AbstractConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; set; }
        public CasTeleplusNetworkAbstractConfigGenerator()
        {
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx,
                new telcobrightpartner
                {
                    idCustomer = 9,
                    CustomerName = "TeleplusNetwork Communications Ltd.",
                    idOperatorType = 2,
                    databasename = "teleplusnetwork_cas",
                    NativeTimeZone = 3251,
                    IgwPrefix = null,
                    RateDictionaryMaxRecords = 3000000,
                    MinMSForIntlOut = 100,
                    RawCdrKeepDurationDays = 90,
                    SummaryKeepDurationDays = 730,
                    AutoDeleteOldData = 1,
                    AutoDeleteStartHour = 4,
                    AutoDeleteEndHour = 6
                });

        }

        public override TelcobrightConfig GenerateConfig(InstanceConfig instanceConfig, int microserviceInstanceId)
        {
            

            CdrSetting tempCdrSetting = new CdrSetting();//helps with getting some values initialized in constructors
            CommonCdrValRulesGen commonCdrValRulesGen =
                new CommonCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            InconsistentCdrValRulesGen inconsistentCdrValRulesGen =
                new InconsistentCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);

            this.Tbc.CdrSetting = new CdrSetting
            {
                SummaryTimeField = SummaryTimeFieldEnum.AnswerTime,
                PartialCdrEnabledNeIds = new List<int>() { },//7, was set to non-partial processing mode due to duplicate billid problem.
                PartialCdrFlagIndicators = new List<string>() { },//{"1", "2", "3"},
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false,
                ValidationRulesForCommonMediationCheck = commonCdrValRulesGen.GetRules(),
                ValidationRulesForInconsistentCdrs = inconsistentCdrValRulesGen.GetRules(),
                ServiceGroupConfigurations = this.GetServiceGroupConfigurations(),
                DisableCdrPostProcessingJobCreationForAutomation = false,
                BatchSizeForCdrJobCreationCheckingExistence = 10000,
                DisableParallelMediation = false,
                AutoCorrectDuplicateBillId = false,
                AutoCorrectBillIdsWithPrevChargeableIssue = true,
                AutoCorrectDuplicateBillIdBeforeErrorProcess = true,
                UseIdCallAsBillId = true,
                ExceptionalCdrPreProcessingData = new Dictionary<string, Dictionary<string, string>>(),
                BatchSizeWhenPreparingLargeSqlJob = 100000,
                
            };
            this.PrepareDirectorySettings(this.Tbc);
            this.Tbc.Nes = new List<ne>()
            {
                new ne
                {
                    idSwitch = 9,
                    idCustomer = this.Tbc.Telcobrightpartner.idCustomer,
                    idcdrformat = 17,
                    idMediationRule = 2,
                    SwitchName = "zte",
                    CDRPrefix = "ICX",
                    FileExtension = ".DAT",
                    Description = null,
                    SourceFileLocations = this.vaultPrimary.Name,
                    BackupFileLocations = null,
                    LoadingStopFlag = null,
                    LoadingSpanCount = 100,
                    TransactionSizeForCDRLoading = 1500,
                    DecodingSpanCount = 100,
                    SkipAutoCreateJob = 1,
                    SkipCdrListed = 0,
                    SkipCdrReceived = 0,
                    SkipCdrDecoded = 0,
                    SkipCdrBackedup = 1,
                    KeepDecodedCDR = 0,
                    KeepReceivedCdrServer = 1,
                    CcrCauseCodeField = 56,
                    SwitchTimeZoneId = null,
                    CallConnectIndicator = "F5",
                    FieldNoForTimeSummary = 29,
                    EnableSummaryGeneration = "1",
                    ExistingSummaryCacheSpanHr = 6,
                    BatchToDecodeRatio = 3,
                    PrependLocationNumberToFileName = 0
                },
                new ne
                {
                    idSwitch = 10,
                    idCustomer = this.Tbc.Telcobrightpartner.idCustomer,
                    idcdrformat = 17,
                    idMediationRule = 2,
                    SwitchName = "Dialogic",
                    CDRPrefix = "sdr",
                    FileExtension = ".gz",
                    Description = null,
                    SourceFileLocations = this.vaultDialogic.Name,
                    BackupFileLocations = null,
                    LoadingStopFlag = null,
                    LoadingSpanCount = 100,
                    TransactionSizeForCDRLoading = 1500,
                    DecodingSpanCount = 100,
                    SkipAutoCreateJob = 1,
                    SkipCdrListed = 0,
                    SkipCdrReceived = 0,
                    SkipCdrDecoded = 0,
                    SkipCdrBackedup = 1,
                    KeepDecodedCDR = 0,
                    KeepReceivedCdrServer = 1,
                    CcrCauseCodeField = 56,
                    SwitchTimeZoneId = null,
                    CallConnectIndicator = "F5",
                    FieldNoForTimeSummary = 29,
                    EnableSummaryGeneration = "1",
                    ExistingSummaryCacheSpanHr = 6,
                    BatchToDecodeRatio = 3,
                    PrependLocationNumberToFileName = 0
                }
            };

            

            this.PrepareProductAndServiceConfiguration();
            this.Tbc.DatabaseSetting = this.GetDatabaseConfigs();
            this.Tbc.ApplicationServersConfig = this.GetServerConfigs();
            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc.Telcobrightpartner.CustomerName);
            return this.Tbc;
        }
    }
}
