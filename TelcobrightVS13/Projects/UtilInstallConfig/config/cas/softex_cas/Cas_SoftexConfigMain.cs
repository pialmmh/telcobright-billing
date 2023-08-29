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
using InstallConfig.config._helper;
using InstallConfig._CommonValidation;
using InstallConfig._generator;
using LibraryExtensions;
using MediationModel;
using TelcobrightInfra;
using TelcobrightMediation.Accounting;

namespace InstallConfig
{
    [Export(typeof(AbstractConfigGenerator))]
    public partial class CasSoftexAbstractConfigGenerator : AbstractConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; set; }
        public CasSoftexAbstractConfigGenerator()
        {
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx,
                new telcobrightpartner
                {
                    idCustomer = 7,
                    CustomerName = "Softex Communications Ltd.",
                    idOperatorType = 2,
                    databasename = "softex_cas",
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
                FileSplitSetting = new FileSplitSetting()
                {
                    FileSplitType = "byte",
                    BytesPerRecord = 559,
                    MaxRecordsInSingleFile = 30000,
                    SplitFileIfSizeBiggerThanMbyte = 559 * 70000//559*30000
                },
            };

            this.PrepareDirectorySettings(this.Tbc);


            string csvPathForNe = new DirectoryInfo(FileAndPathHelper.GetCurrentExecPath()).Parent.Parent.FullName + Path.DirectorySeparatorChar.ToString() + "config" + Path.DirectorySeparatorChar.ToString() + "_helper" + Path.DirectorySeparatorChar.ToString() + "casOperatorInfo.xlsx";//add more
            CasNeInfoHelper neHelper = new CasNeInfoHelper(csvPathForNe);
            this.Tbc.Nes = neHelper.getNesByOpId(this.Tbc.Telcobrightpartner.idCustomer);



            this.PrepareProductAndServiceConfiguration();
            this.Tbc.ApplicationServersConfig = this.GetServerConfigs();
            this.Tbc.DatabaseSetting = this.GetDatabaseConfigs();
            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc);
            return this.Tbc;
        }
    }
}
