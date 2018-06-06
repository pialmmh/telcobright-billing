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
            CommonCdrValRulesGen commonCdrValRulesGen=
                new CommonCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            InconsistentCdrValRulesGen inconsistentCdrValRulesGen =
                new InconsistentCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            this.Tbc.CdrSetting = new CdrSetting()
            {
                SummaryTimeField = SummaryTimeFieldEnum.StartTime,
                PartialCdrEnabledNeIds =new List<int>(),
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false,
                ValidationRulesForInconsistentCdrs = inconsistentCdrValRulesGen.GetRules(),
                ValidationRulesForCommonMediationCheck = commonCdrValRulesGen.GetRules(),
                ServiceGroupConfigurations = this.GetServiceGroupConfigurations(),
                DisableCdrPostProcessingJobCreationForAutomation = true    
            };
            
            this.PrepareDirectorySetting(this.Tbc);

            this.PrepareProductAndServiceSettings();
            
            this.PrepareAppServerSettings();

            DatabaseSetting databaseSetting = schedulerDatabaseSetting.GetCopy();
            databaseSetting.DatabaseName = this.OperatorName;//change dbname here if required
            this.Tbc.DatabaseSetting = databaseSetting;
            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc);
            
            //automation has a interface instanciation issue with json, fix it.
            //this.Tbc.AutomationSetting = GetAutomationSetting();            
            return this.Tbc;
        }
        
    }
}
