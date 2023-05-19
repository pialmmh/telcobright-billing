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
    public partial class IcnlConfigGenerator : IConfigGenerator
    {
        public string OperatorName => this.Tbc.Telcobrightpartner.CustomerName;
        public TelcobrightConfig Tbc { get; }

        public IcnlConfigGenerator()
        {
            int thisServerId = 1;
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Igw, thisServerId);
            this.Tbc.IdTelcobrightPartner = 1;
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
                PartialCdrEnabledNeIds = new List<int>(), //{ 3 },
                PartialCdrFlagIndicators = new List<string>(), //{ "1", "2", "3" },
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false,
                ValidationRulesForCommonMediationCheck = commonCdrValRulesGen.GetRules(),
                ValidationRulesForInconsistentCdrs = inconsistentCdrValRulesGen.GetRules(),
                ServiceGroupPreProcessingRules = this.GetServiceGroupPreProcessingRules(),
                ServiceGroupConfigurations = this.GetServiceGroupConfigurations(),
                DisableCdrPostProcessingJobCreationForAutomation = false,
                DisableParallelMediation = false,
                EnableTgCreationForAns = true
            };

            this.PrepareDirectorySetting(this.Tbc);

            this.PrepareProductAndServiceSettings();

            this.PrepareAppServerSettings();

            DatabaseSetting databaseSetting = schedulerDatabaseSetting.GetCopy();
            databaseSetting.DatabaseName = this.OperatorName;//change dbname here if required
            this.Tbc.DatabaseSetting = databaseSetting;

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
