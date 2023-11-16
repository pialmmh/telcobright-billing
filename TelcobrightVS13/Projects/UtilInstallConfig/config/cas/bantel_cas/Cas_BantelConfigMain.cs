using Newtonsoft.Json;
using System;
using System.IO;
using TelcobrightFileOperations;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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
    public sealed partial class CasBantelAbstractConfigGenerator : AbstractConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; set; }
        public override int IdOperator { get; set; } = 4;
        public override string CustomerName { get; set; } = "Bantel Ltd";
        public override string DatabaseName { get; set; } = "bantel_cas";

        public CasBantelAbstractConfigGenerator()
        {
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx, CasTbPartnerFactory.GetTemplatePartner(this.IdOperator,this.CustomerName,this.DatabaseName));
        }

        public override TelcobrightConfig GenerateFullConfig(InstanceConfig instanceConfig, int microserviceInstanceId)
        {
            this.Tbc.CdrSetting = new CasCdrSettingHelper().getTemplateCdrSettings();
            this.PrepareDirectorySettings(this.Tbc);
            string csvPathForNe = CasNeInfoHelper.getCasOperatorInfoFile();
            CasNeInfoHelper neHelper = new CasNeInfoHelper(csvPathForNe);

            List<NeWrapperWithAdditionalInfo> neWrapperWithAdditionalInfo =
                neHelper.getNesByOpId(this.Tbc.Telcobrightpartner.idCustomer);
            this.Tbc.Nes = neWrapperWithAdditionalInfo.Select(wrapped => wrapped.ne).ToList();

            var neWiseAdditionalSettings = new Dictionary<int, NeAdditionalSetting>();
            foreach (NeWrapperWithAdditionalInfo wrapped in neWrapperWithAdditionalInfo)
            {
                var ne = wrapped.ne;
                var additionalSetting = wrapped.neAdditionalSetting;
                neWiseAdditionalSettings.Add(ne.idSwitch, additionalSetting);
            }
            this.Tbc.CdrSetting.NeWiseAdditionalSettings = neWiseAdditionalSettings;

            this.PrepareProductAndServiceConfiguration();
            this.Tbc.DatabaseSetting = this.GetDatabaseConfigs();
            this.Tbc.ApplicationServersConfig = this.GetServerConfigs();
            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc);
            return this.Tbc;
        }
    }
}
