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
using LogPreProcessor;
using MediationModel;
using TelcobrightInfra;
using TelcobrightMediation.Accounting;

namespace InstallConfig
{
    [Export(typeof(AbstractConfigGenerator))]
    public partial class CCLAbstractConfigGenerator : AbstractConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; set; }
        public override int IdOperator { get; set; }
        public override string CustomerName { get; set; }
        public override string DatabaseName { get; set; }

        public CCLAbstractConfigGenerator()
        {
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx,
                new telcobrightpartner
                {
                    idCustomer = 9,
                    CustomerName = "Cosmopolitan Communications Limited",
                    idOperatorType = 2,
                    databasename = "ccl",
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

        public override TelcobrightConfig GenerateFullConfig(InstanceConfig instanceConfig, int microserviceInstanceId)
        {
            CdrSetting tempCdrSetting = new CdrSetting();//helps with getting some values initialized in constructors
            CommonCdrValRulesGen commonCdrValRulesGen =
                new CommonCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            InconsistentCdrValRulesGen inconsistentCdrValRulesGen =
                new InconsistentCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);

            this.Tbc.CdrSetting = new CdrSetting
            {
                EmptyFileAllowed = true,
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
                ExceptionalCdrPreProcessingData = new Dictionary<string, Dictionary<string, string>>(),
                NeWiseAdditionalSettings = new Dictionary<int, NeAdditionalSetting>
                {
                    { 1, new NeAdditionalSetting {//for TELCOBRIGHT SWITCH
                        ProcessMultipleCdrFilesInBatch = false,
                        PreDecodeAsTextFile = false,
                        MaxConcurrentFilesForParallelPreDecoding = 30,
                        MinRowCountToStartBatchCdrProcessing = 70000,
                        MaxNumberOfFilesInPreDecodedDirectory = 500,
                        EventPreprocessingRules = new List<EventPreprocessingRule>()
                        {
                            new CdrPredecoder()
                            {
                                RuleConfigData = new Dictionary<string,object>() { { "maxParallelFileForPreDecode", "10"}},
                                ProcessCollectionOnly = true//does not accept single event, only list of events e.g. multiple new cdr jobs
                            }
                        }
                    }}
                },
                BatchSizeWhenPreparingLargeSqlJob = 100000,
                WriteFailedCallsToDb = false,
                MoveCdrToDriveAfterProcessing = ""
            };
            this.PrepareDirectorySettings(this.Tbc);
            this.Tbc.Nes = new List<ne>()
            {
                new ne
                {
                    idSwitch = 1,
                    idCustomer = this.Tbc.Telcobrightpartner.idCustomer,
                    idcdrformat = 737,
                    idMediationRule = 2,
                    SwitchName = "Telcobright",
                    CDRPrefix = "cdr",
                    FileExtension = ".csv",
                    Description = null,
                    SourceFileLocations = this.vaultPrimary.Name,
                    BackupFileLocations = null,
                    LoadingStopFlag = null,
                    LoadingSpanCount = 100,
                    TransactionSizeForCDRLoading = 1500,
                    DecodingSpanCount = 1000,
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
                    PrependLocationNumberToFileName = 0,
                    UseIdCallAsBillId = 1,
                    AllowEmptyFile = 1,
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
