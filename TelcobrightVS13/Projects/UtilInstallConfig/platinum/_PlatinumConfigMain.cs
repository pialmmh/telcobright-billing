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
using TelcobrightMediation.Automation;

namespace InstallConfig
{
    [Export(typeof(IConfigGenerator))]
    public partial class PlatinumConfigGenerator:IConfigGenerator
    {
        public string OperatorName { get;}
        public TelcobrightConfig Tbc { get; }
        public PlatinumConfigGenerator()
        {
            int thisServerId = 1;
            this.OperatorName = "platinum";
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Igw, thisServerId);
            this.Tbc.IdTelcobrightPartner = 1;
        }
        public TelcobrightConfig GenerateConfig(DatabaseSetting schedulerDatabaseSetting)
        {
            if (string.IsNullOrWhiteSpace(this.OperatorName))
                throw new Exception("Operator name not configured in Config Generator");
            CdrSetting tempCdrSetting = new CdrSetting();//helps with getting some values initialized in constructors
            BasicCdrValidationRuleGenerator basicValidationRuleGenerator=
                new BasicCdrValidationRuleGenerator(tempCdrSetting.NotAllowedCallDateTimeBefore);

            this.Tbc.CdrSetting = new CdrSetting()
            {
                SummaryTimeField = SummaryTimeFieldEnum.StartTime,
                PartialCdrEnabledNeIds =new List<int>(),
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false,
                ValidationRulesForInconsistentCdrs = basicValidationRuleGenerator.GetInconsistentValidationRules(),
                CommonMediationChecklist = basicValidationRuleGenerator.GetCommonValidationRules(),
                ServiceGroupConfigurations = this.GetServiceGroupConfigurations(),
                DisableCdrPostProcessingJobCreationForAutomation = true    
            };
            
            this.PrepareDirectorySetting(this.Tbc);

            List<KeyValuePair<Regex, string>> serviceAliases = new List<KeyValuePair<Regex, string>>
            {
                new KeyValuePair<Regex, string>(new Regex(@".*/sg5/.*/sf4/.*"), "International Outgoing"),
                new KeyValuePair<Regex, string>(new Regex(@".*/sg4/.*/sf1/.*"), "AZ Voice")
            };
            this.Tbc.ServiceAliasesRegex = serviceAliases;
            
            ApplicationServerConfig serverConfig1 = new ApplicationServerConfig(this.Tbc) { ServerId = 1, OwnIpAddress = "192.168.101.1" };
            ApplicationServerConfig serverConfig2 = new ApplicationServerConfig(this.Tbc) { ServerId = 2 };

            this.Tbc.ApplicationServersConfig.Add(serverConfig1.ServerId.ToString(), serverConfig1);
            this.Tbc.ApplicationServersConfig.Add(serverConfig2.ServerId.ToString(), serverConfig1);

            DatabaseSetting databaseSetting = schedulerDatabaseSetting.GetCopy();
            databaseSetting.DatabaseName = this.OperatorName;//change dbname here if required
            this.Tbc.DatabaseSetting = databaseSetting;
            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc);
            this.Tbc.AutomationSetting = GetAutomationSetting();            
            return this.Tbc;
        }
        
    }
}
