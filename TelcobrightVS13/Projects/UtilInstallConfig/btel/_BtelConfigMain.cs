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
using TelcobrightMediation.Automation;

namespace InstallConfig
{
    [Export(typeof(AbstractConfigConfigGenerator))]
    public partial class BtelAbstractConfigConfigGenerator : AbstractConfigConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; }
        public BtelAbstractConfigConfigGenerator()
        {
            int thisServerId = 1;
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Igw, thisServerId,
                new telcobrightpartner
                {
                    idCustomer = 3,
                    CustomerName = "Banglatel Limited",
                    idOperatorType = 4,
                    databasename = "btel",
                    NativeTimeZone = 3251,
                    IgwPrefix = "320",
                    RateDictionaryMaxRecords = 3000000,
                    MinMSForIntlOut = 100,
                    RawCdrKeepDurationDays = 90,
                    SummaryKeepDurationDays = 730,
                    AutoDeleteOldData = 1,
                    AutoDeleteStartHour = 2,
                    AutoDeleteEndHour = 3
                }, 
                tcpPortNoForRemoteScheduler: 560);
        }

        public override TelcobrightConfig GenerateConfig()
        {
            this.Tbc.Nes = new List<ne>()
            {
                new ne
                {
                    idSwitch= 3,
                    idCustomer= 3,
                    idcdrformat= 16,
                    idMediationRule= 1,
                    SwitchName= "BtelhuaweiDhk",
                    CDRPrefix= "IGW",
                    FileExtension= ".DAT",
                    Description= null,
                    SourceFileLocations= "ftp://127.0.0.1/banglatel/",
                    BackupFileLocations= null,
                    LoadingStopFlag= null,
                    LoadingSpanCount= 100,
                    TransactionSizeForCDRLoading= 1500,
                    DecodingSpanCount= 100,
                    SkipAutoCreateJob= 1,
                    SkipCdrListed= 1,
                    SkipCdrReceived= 1,
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
                },
            };

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
                DisableCdrPostProcessingJobCreationForAutomation = false,
                DisableParallelMediation = false,
                AutoCorrectBillIdsWithPrevChargeableIssue=true,
                AutoCorrectDuplicateBillId=true,
                AutoCorrectDuplicateBillIdBeforeErrorProcess = true,
                UseIdCallAsBillId=true
            };

            this.PrepareDirectorySetting(this.Tbc);
            this.PrepareProductAndServiceSettings();
            this.Tbc.ApplicationServersConfig = this.GetApplicationServerConfigs();
            this.Tbc.DatabaseSetting = this.GetDatabaseSettings();
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

            return this.Tbc;
        }

    }
}
