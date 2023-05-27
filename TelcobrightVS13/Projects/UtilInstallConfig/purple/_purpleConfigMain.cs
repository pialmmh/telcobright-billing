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
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InstallConfig
{
    [Export(typeof(AbstractConfigConfigGenerator))]
    public partial class PurpleAbstractConfigConfigGenerator:AbstractConfigConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; }
        public PurpleAbstractConfigConfigGenerator()
        {
            int thisServerId = 1;
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx, thisServerId,
                new telcobrightpartner
                    {
                        idCustomer = 2,
                        CustomerName = "Purple Telecom Ltd.",
                        idOperatorType = 2,
                        databasename = "purple",
                        databasetype = "mysql",
                        user = "root",
                        pass = null,
                        ServerNameOrIP = "127.0.0.1:3306",
                        IBServerNameOrIP = "127.0.0.1:3306",
                        IBdatabasename = "roots",
                        IBdatabasetype = "InfoBright",
                        IBuser = "root",
                        IBpass = null,
                        TransactionSizeForCDRLoading = 1500,
                        NativeTimeZone = 3251,
                        IgwPrefix = "190",
                        RateDictionaryMaxRecords = 3000000,
                        MinMSForIntlOut = 100,
                        RawCdrKeepDurationDays = 90,
                        SummaryKeepDurationDays = 730,
                        AutoDeleteOldData = 1,
                        AutoDeleteStartHour = 4,
                        AutoDeleteEndHour = 6
                    },
                tcpPortNoForRemoteScheduler: 557
                );
        }
        public override TelcobrightConfig GenerateConfig()
        {
            this.Tbc.Nes = new List<ne>()
            {
                new ne
                {
                    idSwitch= 2,
                    idCustomer= 2,
                    idcdrformat= 3,
                    idMediationRule= 2,
                    SwitchName= "dhkHuawei",
                    CDRPrefix= "p",
                    FileExtension= ".dat",
                    Description= null,
                    SourceFileLocations= "[{  'type':'ftp','url':'ftp://127.0.0.1/' , 'user':'ftpuser','pass':'Takay1takaane','usefullurl':'false' }]",
                    BackupFileLocations= null,
                    LoadingStopFlag= null,
                    LoadingSpanCount= 10000,
                    TransactionSizeForCDRLoading= 100,
                    DecodingSpanCount= 10000,
                    SkipAutoCreateJob= 1,
                    SkipCdrListed= 1,
                    SkipCdrReceived= 1,
                    SkipCdrDecoded= 1,
                    SkipCdrBackedup= 1,
                    KeepDecodedCDR= 0,
                    KeepReceivedCdrServer= 1,
                    CcrCauseCodeField= 56,
                    SwitchTimeZoneId= null,
                    CallConnectIndicator= "CT",
                    FieldNoForTimeSummary= 29,
                    EnableSummaryGeneration= "1",
                    ExistingSummaryCacheSpanHr= 6,
                    BatchToDecodeRatio= 3,
                    PrependLocationNumberToFileName= 0
                }
            };

            CdrSetting tempCdrSetting = new CdrSetting();//helps with getting some values initialized in constructors
            CommonCdrValRulesGen commonCdrValRulesGen =
                new CommonCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            InconsistentCdrValRulesGen inconsistentCdrValRulesGen =
                new InconsistentCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            this.Tbc.CdrSetting = new CdrSetting
            {
                SummaryTimeField = SummaryTimeFieldEnum.AnswerTime,
                PartialCdrEnabledNeIds = new List<int>(),
                PartialCdrFlagIndicators = new List<string>(), //{"1", "2", "3"},
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false,
                ValidationRulesForCommonMediationCheck = commonCdrValRulesGen.GetRules(),
                ValidationRulesForInconsistentCdrs = inconsistentCdrValRulesGen.GetRules(),
                ServiceGroupConfigurations = this.GetServiceGroupConfigurations(),
                DisableCdrPostProcessingJobCreationForAutomation = false,
                DisableParallelMediation = false,
                AutoCorrectDuplicateBillId = true,
                AutoCorrectBillIdsWithPrevChargeableIssue = true,
                UseIdCallAsBillId=true,
            };

            this.PrepareDirectorySettings(this.Tbc);

            this.PrepareProductAndServiceConfiguration();
            
            this.PrepareApplicationServerConfig();
            this.Tbc.DatabaseSetting = this.GetDatabaseSettings();

            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc);
            return this.Tbc;
        }
    }
}
