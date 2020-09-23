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
using TelcobrightMediation.Accounting;

namespace InstallConfig
{
    [Export(typeof(IConfigGenerator))]
    public partial class JslConfigGenerator:IConfigGenerator
    {
        public string OperatorName { get;}
        public TelcobrightConfig Tbc { get; }

        public JslConfigGenerator()
        {
            int thisServerId = 1;
            this.OperatorName = "jsl";
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx, thisServerId);
        }

        public TelcobrightConfig GenerateConfig(DatabaseSetting schedulerDatabaseSetting)
        {
            if (string.IsNullOrWhiteSpace(this.OperatorName))
                throw new Exception("Operator name not configured in Config Generator");

            CdrSetting tempCdrSetting = new CdrSetting();//helps with getting some values initialized in constructors
            CommonCdrValRulesGen commonCdrValRulesGen =
                new CommonCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            InconsistentCdrValRulesGen inconsistentCdrValRulesGen =
                new InconsistentCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            this.Tbc.CdrSetting = new CdrSetting
            {
                SummaryTimeField = SummaryTimeFieldEnum.AnswerTime,
                PartialCdrEnabledNeIds = new List<int>() {7},
                PartialCdrFlagIndicators = new List<string>() {"1", "2", "3"},
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false,
                ValidationRulesForCommonMediationCheck = commonCdrValRulesGen.GetRules(),
                ValidationRulesForInconsistentCdrs = inconsistentCdrValRulesGen.GetRules(),
                ServiceGroupConfigurations = this.GetServiceGroupConfigurations(),
                DisableCdrPostProcessingJobCreationForAutomation = false,
                DisableParallelMediation = false,
                AutoCorrectDuplicateBillId = true,
                AutoCorrectBillIdsWithPrevChargeableIssue = true,
                FileSplitSetting = new FileSplitSetting()
                {
                    FileSplitType = "byte",
                    BytesPerRecord = 559,
                    MaxRecordsInSingleFile = 30000,
                    SplitFileIfSizeBiggerThanMbyte = 559*30000
                },
                ExceptionalCdrPreProcessingData = new Dictionary<string, Dictionary<string, string>>()
                {
                    { "DateChangerForCdr",new Dictionary<string, string>()
                        {
                            { "random","true"},
                            { "changeDateStart","2020-09-08 15:00:00" },
                            { "changeDateEnd","2020-09-08 1:00:00" },
                        }
                    }
                }
            };

            this.PrepareDirectorySettings(this.Tbc);

            this.PrepareProductAndServiceConfiguration();
            
            this.PrepareApplicationServerConfig();

            DatabaseSetting databaseSetting = schedulerDatabaseSetting.GetCopy();
            databaseSetting.DatabaseName = this.OperatorName;//change dbname here if required
            this.Tbc.DatabaseSetting = databaseSetting;

            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc);
            return this.Tbc;
        }
    }
}
