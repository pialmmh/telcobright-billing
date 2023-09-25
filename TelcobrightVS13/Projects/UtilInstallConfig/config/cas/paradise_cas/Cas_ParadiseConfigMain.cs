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
    public sealed partial class CasParadiseAbstractConfigGenerator : AbstractConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; set; }
        //public override string CustomerName { get; set; } = "Paradise Telecom";
        //public override string DatabaseName { get; set; } = "paradise_cas";
        public int IdOperator { get; set; } = 15;
        public string CustomerName { get; set; } = "Paradise Telecom";
        public string DatabaseName { get; set; } = "paradise_cas";

        public CasParadiseAbstractConfigGenerator()
        {
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx,CasTbPartnerFactory.GetTemplatePartner(15, this.CustomerName, this.DatabaseName));
        }

        public override TelcobrightConfig GenerateFullConfig(InstanceConfig instanceConfig, int microserviceInstanceId)
        {
            var newTbc = new TelcobrightConfig(TelecomOperatortype.Icx, CasTbPartnerFactory
                .GetTemplatePartner(this.IdOperator, this.CustomerName, this.DatabaseName));
            newTbc.CdrSetting =new CasCdrSettingHelper().getTemplateCdrSettings();
            this.PrepareDirectorySettings(newTbc);

            string csvPathForNe = new DirectoryInfo(FileAndPathHelper.GetCurrentExecPath()).Parent.Parent.FullName + Path.DirectorySeparatorChar.ToString() + "config" + Path.DirectorySeparatorChar.ToString() + "_helper" + Path.DirectorySeparatorChar.ToString() + "casOperatorInfo.xlsx";//add more
            CasNeInfoHelper neHelper = new CasNeInfoHelper(csvPathForNe);
            newTbc.Nes = neHelper.getNesByOpId(this.IdOperator);
            this.PrepareProductAndServiceConfiguration();
            newTbc.ApplicationServersConfig = this.GetServerConfigs();
            newTbc.DatabaseSetting = this.GetDatabaseConfigs();
            newTbc.PortalSettings = GetPortalSettings(this.Tbc);
            return newTbc;
        }
    }
}
