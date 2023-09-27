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
    public sealed partial class CasGaziNetworksAbstractConfigGenerator : AbstractConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; set; }
        public override int IdOperator { get; set; } = 6;
        public override string CustomerName { get; set; } = "Gazi Networks Ltd";
        public override string DatabaseName { get; set; } = "gazinetworks_cas";

        public CasGaziNetworksAbstractConfigGenerator()
        {
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx,CasTbPartnerFactory.GetTemplatePartner(this.IdOperator,this.CustomerName,this.DatabaseName));
        }

        public override TelcobrightConfig GenerateFullConfig(InstanceConfig instanceConfig, int microserviceInstanceId)
        {
            this.Tbc.CdrSetting = new CasCdrSettingHelper().getTemplateCdrSettings();
            this.PrepareDirectorySettings(this.Tbc);


            string csvPathForNe = CasNeInfoHelper.getCasOperatorInfoFile();
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
