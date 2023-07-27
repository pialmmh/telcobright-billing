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
    public partial class CasBantelAbstractConfigGenerator : AbstractConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; set; }
        public CasBantelAbstractConfigGenerator()
        {
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx,
                new telcobrightpartner
                {
                    idCustomer = 7,
                    CustomerName = "Bantel Solutions Ltd.",
                    idOperatorType = 2,
                    databasename = "bantel_cas",
                    databasetype = "mysql",
                    user = "root",
                    pass = null,
                    ServerNameOrIP = null,
                    IBServerNameOrIP = null,
                    IBdatabasename = null,
                    IBdatabasetype = null,
                    IBuser = null,
                    IBpass = null,
                    TransactionSizeForCDRLoading = null,
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
                DisableParallelMediation = false,
                AutoCorrectDuplicateBillId = true,
                AutoCorrectBillIdsWithPrevChargeableIssue = true,
                AutoCorrectDuplicateBillIdBeforeErrorProcess = true,
                UseIdCallAsBillId = true,
                FileSplitSetting = new FileSplitSetting()
                {
                    FileSplitType = "byte",
                    BytesPerRecord = 559,
                    MaxRecordsInSingleFile = 30000,
                    SplitFileIfSizeBiggerThanMbyte = 559 * 70000//559*30000
                },
            };

            this.PrepareDirectorySettings(this.Tbc);
            this.Tbc.Nes = new List<ne>()
            {
                new ne
                {
                    idSwitch= 1,
                    idCustomer= this.Tbc.Telcobrightpartner.idCustomer,
                    idcdrformat= 17,
                    idMediationRule= 2,
                    SwitchName= "huwaei",
                    CDRPrefix= "ICX",
                    FileExtension= ".DAT",
                    Description= null,
                    SourceFileLocations= "Vault.JslZteDhk",
                    BackupFileLocations= null,
                    LoadingStopFlag= null,
                    LoadingSpanCount= 100,
                    TransactionSizeForCDRLoading= 1500,
                    DecodingSpanCount= 100,
                    SkipAutoCreateJob= 1,
                    SkipCdrListed= 0,
                    SkipCdrReceived= 1,
                    SkipCdrDecoded= 0,
                    SkipCdrBackedup= 1,
                    KeepDecodedCDR= 1,
                    KeepReceivedCdrServer= 1,
                    CcrCauseCodeField= 56,
                    SwitchTimeZoneId= null,
                    CallConnectIndicator= "F5",
                    FieldNoForTimeSummary= 29,
                    EnableSummaryGeneration= "1",
                    ExistingSummaryCacheSpanHr= 6,
                    BatchToDecodeRatio= 3,
                    PrependLocationNumberToFileName= 0
                },
                new ne
                {
                    idSwitch= 2,
                    idCustomer= this.Tbc.Telcobrightpartner.idCustomer,
                    idcdrformat= 25,
                    idMediationRule= 2,
                    SwitchName= "cataleya",
                    CDRPrefix= "esdr",
                    FileExtension= ".txt",
                    Description= null,
                    SourceFileLocations= "Vault.JslcataleyaDhk",
                    BackupFileLocations= null,
                    LoadingStopFlag= null,
                    LoadingSpanCount= 100,
                    TransactionSizeForCDRLoading= 1500,
                    DecodingSpanCount= 100,
                    SkipAutoCreateJob= 1,
                    SkipCdrListed= 1,
                    SkipCdrReceived= 0,
                    SkipCdrDecoded= 1,
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
                    PrependLocationNumberToFileName= 0
                }
            };

            
            this.PrepareProductAndServiceConfiguration();
            this.Tbc.ApplicationServersConfig = this.GetServerConfigs();
            this.Tbc.DatabaseSetting = this.GetDatabaseConfigs();
            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc);
            return this.Tbc;
        }
    }
}
