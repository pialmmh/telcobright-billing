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
            BasicCdrValidationRuleGenerator basicValidationRuleGenerator =
                new BasicCdrValidationRuleGenerator(tempCdrSetting.NotAllowedCallDateTimeBefore);
            this.Tbc.CdrSetting = new CdrSetting
            {
                SummaryTimeField = SummaryTimeFieldEnum.AnswerTime,
                PartialCdrEnabledNeIds = new List<int>() {7},
                PartialCdrFlagIndicators = new List<string>() {"1", "2", "3"},
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false,
                ValidationRulesForInconsistentCdrs = basicValidationRuleGenerator.GetInconsistentValidationRules(),
                CommonMediationChecklist = basicValidationRuleGenerator.GetCommonValidationRules(),
                ServiceGroupConfigurations = this.GetServiceGroupConfigurations(),
                DisableCdrPostProcessingJobCreationForAutomation = false
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
