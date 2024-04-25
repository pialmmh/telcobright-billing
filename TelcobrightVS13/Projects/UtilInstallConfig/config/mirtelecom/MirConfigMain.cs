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
using TelcobrightMediation.Automation;
using LogPreProcessor;

namespace InstallConfig
{
    [Export(typeof(AbstractConfigGenerator))]
    public partial class MirAbstractConfigGenerator : AbstractConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; set; }
        public override int IdOperator { get; set; }
        public override string CustomerName { get; set; }
        public override string DatabaseName { get; set; }

        public MirAbstractConfigGenerator()
        {
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Igw,
                new telcobrightpartner
                {
                    idCustomer = 10,
                    CustomerName = "Mirtelecom Limited",
                    idOperatorType = 4,
                    databasename = "mirtelecom",
                    NativeTimeZone = 3251,
                    IgwPrefix = "320",
                    RateDictionaryMaxRecords = 3000000,
                    MinMSForIntlOut = 100,
                    RawCdrKeepDurationDays = 90,
                    SummaryKeepDurationDays = 730,
                    AutoDeleteOldData = 1,
                    AutoDeleteStartHour = 2,
                    AutoDeleteEndHour = 3
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
                SummaryTimeField = SummaryTimeFieldEnum.StartTime,
                PartialCdrEnabledNeIds = new List<int>() { 3 },
                PartialCdrFlagIndicators = new List<string>() { "1", "2", "3" },
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false,
                ValidationRulesForCommonMediationCheck = commonCdrValRulesGen.GetRules(),
                ValidationRulesForInconsistentCdrs = inconsistentCdrValRulesGen.GetRules(),
                ServiceGroupConfigurations = this.GetServiceGroupConfigurations(),
                ServiceGroupPreProcessingRules = new List<int>() {2},
                DisableCdrPostProcessingJobCreationForAutomation = false,
                DisableParallelMediation = false,
                AutoCorrectBillIdsWithPrevChargeableIssue = true,
                AutoCorrectDuplicateBillId = true,
                AutoCorrectDuplicateBillIdBeforeErrorProcess = true,
                EmptyFileAllowed = true,
                NeWiseAdditionalSettings = new Dictionary<int, NeAdditionalSetting>
                {
                {
                    11,
                    new NeAdditionalSetting
                    {//dialogic
                        ProcessMultipleCdrFilesInBatch = false,
                        PreDecodeAsTextFile = false,
                        MaxConcurrentFilesForParallelPreDecoding = 10,
                        MinRowCountToStartBatchCdrProcessing = 100000,
                        MaxNumberOfFilesInPreDecodedDirectory = 500,
                        EventPreprocessingRules = new List<EventPreprocessingRule>()
                        {
                            new CdrPredecoder()
                            {
                                RuleConfigData = new Dictionary<string,object>() { { "maxParallelFileForPreDecode", "100"}},
                                ProcessCollectionOnly = true//does not accept single event, only list of events e.g. multiple new cdr jobs
                            }
                        }
                    }
                },
                {
                    12,
                    new NeAdditionalSetting
                    {// huawei
                        ProcessMultipleCdrFilesInBatch = false,
                        PreDecodeAsTextFile = false,
                        MaxConcurrentFilesForParallelPreDecoding = 10,
                        MinRowCountToStartBatchCdrProcessing = 100000,
                        MaxNumberOfFilesInPreDecodedDirectory = 500,
                        EventPreprocessingRules = new List<EventPreprocessingRule>()
                        {
                            new CdrPredecoder()
                            {
                                RuleConfigData = new Dictionary<string,object>() { { "maxParallelFileForPreDecode", "100"}},
                                ProcessCollectionOnly = true//does not accept single event, only list of events e.g. multiple new cdr jobs
                            }
                        }
                    }
                }
            }
            };

            this.PrepareDirectorySetting(this.Tbc);
            this.Tbc.Nes = new List<ne>()
            {
                new ne
                {
                    idSwitch= 11,
                    idCustomer= this.Tbc.Telcobrightpartner.idCustomer,
                    idcdrformat= 235,
                    idMediationRule= 2,
                    SwitchName= "Dialogic",
                    CDRPrefix= "sdr",
                    FileExtension= ".gz",
                    Description= null,
                    SourceFileLocations= this.vaultDialogic.Name,
                    BackupFileLocations= null,
                    LoadingStopFlag= null,
                    LoadingSpanCount= 100,
                    TransactionSizeForCDRLoading= 1500,
                    DecodingSpanCount= 100,
                    SkipAutoCreateJob= 1,
                    SkipCdrListed= 0,
                    SkipCdrReceived= 0,
                    SkipCdrDecoded= 0,
                    SkipCdrBackedup= 1,
                    KeepDecodedCDR= 0,
                    KeepReceivedCdrServer= 1,
                    CcrCauseCodeField= 56,
                    SwitchTimeZoneId= null,
                    CallConnectIndicator= "F5",
                    FieldNoForTimeSummary= 29,
                    EnableSummaryGeneration= "1",
                    ExistingSummaryCacheSpanHr= 6,
                    BatchToDecodeRatio= 3,
                    PrependLocationNumberToFileName= 0,
                    FilterDuplicateCdr = 1,
                    UseIdCallAsBillId = 0,
                    AllowEmptyFile = 1
                },
                new ne
                {
                    idSwitch= 12,
                    idCustomer= this.Tbc.Telcobrightpartner.idCustomer,
                    idcdrformat= 3,
                    idMediationRule= 2,
                    SwitchName= "Huawei",
                    CDRPrefix= "b",
                    FileExtension= ".dat",
                    Description= null,
                    SourceFileLocations= this.vaultHuawei.Name,
                    BackupFileLocations= null,
                    LoadingStopFlag= null,
                    LoadingSpanCount= 100,
                    TransactionSizeForCDRLoading= 1500,
                    DecodingSpanCount= 100,
                    SkipAutoCreateJob= 1,
                    SkipCdrListed= 0,
                    SkipCdrReceived= 0,
                    SkipCdrDecoded= 0,
                    SkipCdrBackedup= 1,
                    KeepDecodedCDR= 0,
                    KeepReceivedCdrServer= 1,
                    CcrCauseCodeField= 56,
                    SwitchTimeZoneId= null,
                    CallConnectIndicator= "F5",
                    FieldNoForTimeSummary= 29,
                    EnableSummaryGeneration= "1",
                    ExistingSummaryCacheSpanHr= 6,
                    BatchToDecodeRatio= 3,
                    PrependLocationNumberToFileName= 0,
                    FilterDuplicateCdr = 0,
                    UseIdCallAsBillId = 1,
                    AllowEmptyFile = 1
                }

            };


            this.PrepareProductAndServiceSettings();
            this.Tbc.ApplicationServersConfig = this.GetServerConfigs();
            this.Tbc.DatabaseSetting = this.GetDatabaseConfigs();
            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc);

            this.Tbc.EmailSenderConfig = new EmailSenderConfig
            {
                SmtpHost = "mail.telcobright.com",
                SmtpPort = 25,
                Username = "noreply@telcobright.com",
                Password = "myPassword",
                MailFrom = "noreply@telcobright.com",
                MailFromDisplayName = "Telcobright Billing Portal",
                EnableSsl = true
            };

            this.Tbc.SmsSenderConfig = new SmsSenderConfig
            {
                ApiUrl = "http://entsms.microntechbd.com:8080/bulksms/personalizedbulksms",
                Username = "MTLBilling",
                Password = "12345678",
                DestinationNumber = new List<string>
                {
                    "8801779747913",
                    "8801866464603",
                    "8801730329050",
                    "8801755588298",
                    "8801730739302",
                    "8801730795764",
                },
                Source = "8809601000201",
                Message = "Dear Valued Partner, \nOur system indicates that your cdr process has been stopped since one hour.\nTelcobright Billing Portal"
            };

            return this.Tbc;
        }

    }
}
