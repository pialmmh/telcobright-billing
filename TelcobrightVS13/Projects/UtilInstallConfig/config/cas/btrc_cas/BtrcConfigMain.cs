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
using MediationModel;
using TelcobrightInfra;
using TelcobrightMediation.Accounting;

namespace InstallConfig
{
    [Export(typeof(AbstractConfigGenerator))]
    public sealed partial class BtrcAbstractConfigGenerator : AbstractConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; set; }
        public override int IdOperator { get; set; } = 30;
        public override string CustomerName { get; set; } = "BTRC- CDR Analyze System (CAS)";
        public override string DatabaseName { get; set; } = "btrc_cas";

        public BtrcAbstractConfigGenerator()
        {
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx,CasTbPartnerFactory.GetTemplatePartner(this.IdOperator, this.CustomerName, this.DatabaseName));
        }

        public override TelcobrightConfig GenerateFullConfig(InstanceConfig instanceConfig, int microserviceInstanceId)
        {
            
            this.Tbc.CdrSetting = new CasCdrSettingHelper().getTemplateCdrSettings();
            this.PrepareDirectorySettings(this.Tbc);
            this.Tbc.Nes = new List<ne>()
            {
                //new ne
                //{
                //    idSwitch = 9,
                //    idCustomer = this.Tbc.Telcobrightpartner.idCustomer,
                //    idcdrformat = 17,
                //    idMediationRule = 2,
                //    SwitchName = "zte",
                //    CDRPrefix = "ICX",
                //    FileExtension = ".DAT",
                //    Description = null,
                //    SourceFileLocations = this.vaultPrimary.Name,
                //    BackupFileLocations = null,
                //    LoadingStopFlag = null,
                //    LoadingSpanCount = 100,
                //    TransactionSizeForCDRLoading = 1500,
                //    DecodingSpanCount = 100,
                //    SkipAutoCreateJob = 1,
                //    SkipCdrListed = 0,
                //    SkipCdrReceived = 0,
                //    SkipCdrDecoded = 0,
                //    SkipCdrBackedup = 1,
                //    KeepDecodedCDR = 0,
                //    KeepReceivedCdrServer = 1,
                //    CcrCauseCodeField = 56,
                //    SwitchTimeZoneId = null,
                //    CallConnectIndicator = "F5",
                //    FieldNoForTimeSummary = 29,
                //    EnableSummaryGeneration = "1",
                //    ExistingSummaryCacheSpanHr = 6,
                //    BatchToDecodeRatio = 3,
                //    PrependLocationNumberToFileName = 0,
                //    UseIdCallAsBillId = 1,

                //},
                //new ne
                //{
                //    idSwitch = 10,
                //    idCustomer = this.Tbc.Telcobrightpartner.idCustomer,
                //    idcdrformat = 17,
                //    idMediationRule = 2,
                //    SwitchName = "Dialogic",
                //    CDRPrefix = "sdr",
                //    FileExtension = ".gz",
                //    Description = null,
                //    SourceFileLocations = this.vaultDialogic.Name,
                //    BackupFileLocations = null,
                //    LoadingStopFlag = null,
                //    LoadingSpanCount = 100,
                //    TransactionSizeForCDRLoading = 1500,
                //    DecodingSpanCount = 100,
                //    SkipAutoCreateJob = 1,
                //    SkipCdrListed = 0,
                //    SkipCdrReceived = 0,
                //    SkipCdrDecoded = 0,
                //    SkipCdrBackedup = 1,
                //    KeepDecodedCDR = 0,
                //    KeepReceivedCdrServer = 1,
                //    CcrCauseCodeField = 56,
                //    SwitchTimeZoneId = null,
                //    CallConnectIndicator = "F5",
                //    FieldNoForTimeSummary = 29,
                //    EnableSummaryGeneration = "1",
                //    ExistingSummaryCacheSpanHr = 6,
                //    BatchToDecodeRatio = 3,
                //    PrependLocationNumberToFileName = 0,
                //    UseIdCallAsBillId = 1,

                //}
            };

            

            this.PrepareProductAndServiceConfiguration();
            this.Tbc.DatabaseSetting = this.GetDatabaseConfigs();
            this.Tbc.ApplicationServersConfig = this.GetServerConfigs();
            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc.Telcobrightpartner.CustomerName);
            return this.Tbc;
        }
    }
}
