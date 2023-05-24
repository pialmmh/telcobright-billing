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
using MediationModel;
using TelcobrightMediation.Accounting;

namespace InstallConfig
{
    [Export(typeof(AbstractConfigConfigGenerator))]
    public partial class JslAbstractConfigConfigGenerator:AbstractConfigConfigGenerator
    {
        public override TelcobrightConfig Tbc { get; }

        public JslAbstractConfigConfigGenerator()
        {
            int thisServerId = 1;
            this.Tbc = new TelcobrightConfig(TelecomOperatortype.Icx, thisServerId, "Jibondhara Solutions Ltd.");
            this.Tbc.Telcobrightpartner = new telcobrightpartner
            {
                idCustomer = 7,
                CustomerName = this.Tbc.OperatorName,
                idOperatorType = 2,
                databasename = "jsl",
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
            };
        }

        public override TelcobrightConfig GenerateConfig()
        {
            
            this.Tbc.Nes = new List<ne>()
            {
                new ne
                {
                    idSwitch= 7,
                    idCustomer= 7,
                    idcdrformat= 17,
                    idMediationRule= 2,
                    SwitchName= "JslZteDhk",
                    CDRPrefix= "ICX",
                    FileExtension= ".DAT",
                    Description= null,
                    SourceFileLocations= "Vault.JslZteDhk",
                    BackupFileLocations= null,
                    LoadingStopFlag= null,
                    LoadingSpanCount= 100,
                    TransactionSizeForCDRLoading= 1500,
                    DecodingSpanCount= 100,
                    SkipAutoCreateJob= 1,
                    SkipCdrListed= 1,
                    SkipCdrReceived= 1,
                    SkipCdrDecoded= 1,
                    SkipCdrBackedup= 1,
                    KeepDecodedCDR= 1,
                    KeepReceivedCdrServer= 1,
                    CcrCauseCodeField= 56,
                    SwitchTimeZoneId= null,
                    CallConnectIndicator= "F5",
                    FieldNoForTimeSummary= 29,
                    EnableSummaryGeneration= "1",
                    ExistingSummaryCacheSpanHr= 6,
                    BatchToDecodeRatio= 3,
                    PrependLocationNumberToFileName= 0
                }
            };

            CdrSetting tempCdrSetting = new CdrSetting();//helps with getting some values initialized in constructors
            CommonCdrValRulesGen commonCdrValRulesGen =
                new CommonCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            InconsistentCdrValRulesGen inconsistentCdrValRulesGen =
                new InconsistentCdrValRulesGen(tempCdrSetting.NotAllowedCallDateTimeBefore);
            this.Tbc.CdrSetting = new CdrSetting
            {
                SummaryTimeField = SummaryTimeFieldEnum.AnswerTime,
                PartialCdrEnabledNeIds = new List<int>() {},//7, was set to non-partial processing mode due to duplicate billid problem.
                PartialCdrFlagIndicators = new List<string>() {},//{"1", "2", "3"},
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
                UseIdCallAsBillId = true,
                FileSplitSetting = new FileSplitSetting()
                {
                    FileSplitType = "byte",
                    BytesPerRecord = 559,
                    MaxRecordsInSingleFile = 30000,
                    SplitFileIfSizeBiggerThanMbyte = 559 * 70000//559*30000
                },
                ExceptionalCdrPreProcessingData = new Dictionary<string, Dictionary<string, string>>()
                {
                    { "DateChangerForCdr",new Dictionary<string, string>()
                        {
                            { "random","true"},
                            { "changeDateStart","2020-09-08 15:00:00" },
                            { "changeDateEnd","2020-09-08 17:00:00" },
                            { "jobNames",$@"ICX19200824135037682.DAT,			
                                            ICX19200824141537763.DAT,			
                                            ICX19200824140537761_0001.DAT,
                                            ICX19200908174537679_0001.DAT,
                                            ICX19200908174537679_0002.DAT,
                                            ICX19200908174537679_0003.DAT,
                                            ICX19200908174537679_0004.DAT,
                                            ICX19200908174537679_0005.DAT,
                                            ICX19200908174537679_0006.DAT,
                                            ICX19200908174537679_0007.DAT,
                                            ICX19200908174537679_0008.DAT,
                                            ICX19200908174537679_0009.DAT,
                                            ICX19200908174537679_0010.DAT,
                                            ICX19200908174537679_0011.DAT,
                                            ICX19200908174537679_0012.DAT,
                                            ICX19200908174537679_0013.DAT,
                                            ICX19200908174537679_0014.DAT,
                                            ICX19200908174537679_0015.DAT,
                                            ICX19200908174537679_0016.DAT,
                                            ICX19200908174537679_0017.DAT,
                                            ICX19200908174537679_0018.DAT,
                                            ICX19200908174537679_0019.DAT,
                                            ICX19200908174537679_0020.DAT,
                                            ICX19200908174537679_0021.DAT,
                                            ICX19200908174537679_0022.DAT,
                                            ICX19200908174537679_0023.DAT,
                                            ICX19200908174537679_0024.DAT,
                                            ICX19200908174537679_0025.DAT,
                                            ICX19200908174537679_0026.DAT"
                            }
                        }
                    },
                    { "IdCallChangerForCdr",new Dictionary<string, string>()
                        {
                            { "jobNames",$@"ICX19200824135037682.DAT,			
                                            ICX19200824141537763.DAT,			
                                            ICX19200824140537761_0001.DAT,
                                            ICX19200908174537679_0001.DAT,
                                            ICX19200908174537679_0002.DAT,
                                            ICX19200908174537679_0003.DAT,
                                            ICX19200908174537679_0004.DAT,
                                            ICX19200908174537679_0005.DAT,
                                            ICX19200908174537679_0006.DAT,
                                            ICX19200908174537679_0007.DAT,
                                            ICX19200908174537679_0008.DAT,
                                            ICX19200908174537679_0009.DAT,
                                            ICX19200908174537679_0010.DAT,
                                            ICX19200908174537679_0011.DAT,
                                            ICX19200908174537679_0012.DAT,
                                            ICX19200908174537679_0013.DAT,
                                            ICX19200908174537679_0014.DAT,
                                            ICX19200908174537679_0015.DAT,
                                            ICX19200908174537679_0016.DAT,
                                            ICX19200908174537679_0017.DAT,
                                            ICX19200908174537679_0018.DAT,
                                            ICX19200908174537679_0019.DAT,
                                            ICX19200908174537679_0020.DAT,
                                            ICX19200908174537679_0021.DAT,
                                            ICX19200908174537679_0022.DAT,
                                            ICX19200908174537679_0023.DAT,
                                            ICX19200908174537679_0024.DAT,
                                            ICX19200908174537679_0025.DAT,
                                            ICX19200908174537679_0026.DAT"
                            }
                        }
                    },
                    { "PartialToNonPartialChangerForCdr",new Dictionary<string, string>()
                        {
                            { "jobNames",$@"ICX19200824135037682.DAT,			
                                            ICX19200824141537763.DAT,			
                                            ICX19200824140537761_0001.DAT,
                                            ICX19200908174537679_0001.DAT,
                                            ICX19200908174537679_0002.DAT,
                                            ICX19200908174537679_0003.DAT,
                                            ICX19200908174537679_0004.DAT,
                                            ICX19200908174537679_0005.DAT,
                                            ICX19200908174537679_0006.DAT,
                                            ICX19200908174537679_0007.DAT,
                                            ICX19200908174537679_0008.DAT,
                                            ICX19200908174537679_0009.DAT,
                                            ICX19200908174537679_0010.DAT,
                                            ICX19200908174537679_0011.DAT,
                                            ICX19200908174537679_0012.DAT,
                                            ICX19200908174537679_0013.DAT,
                                            ICX19200908174537679_0014.DAT,
                                            ICX19200908174537679_0015.DAT,
                                            ICX19200908174537679_0016.DAT,
                                            ICX19200908174537679_0017.DAT,
                                            ICX19200908174537679_0018.DAT,
                                            ICX19200908174537679_0019.DAT,
                                            ICX19200908174537679_0020.DAT,
                                            ICX19200908174537679_0021.DAT,
                                            ICX19200908174537679_0022.DAT,
                                            ICX19200908174537679_0023.DAT,
                                            ICX19200908174537679_0024.DAT,
                                            ICX19200908174537679_0025.DAT,
                                            ICX19200908174537679_0026.DAT"
                            }
                        }
                    }
                }
            };

            this.PrepareDirectorySettings(this.Tbc);

            this.PrepareProductAndServiceConfiguration();
            
            this.PrepareApplicationServerConfig();

            DatabaseSetting databaseSetting = this.GetDatabaseSettings();
            databaseSetting.DatabaseName = this.Tbc.OperatorName;//change dbname here if required
            this.Tbc.DatabaseSetting = databaseSetting;

            this.Tbc.PortalSettings = GetPortalSettings(this.Tbc);
            return this.Tbc;
        }
    }
}
