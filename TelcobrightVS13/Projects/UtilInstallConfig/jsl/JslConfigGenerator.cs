using Newtonsoft.Json;
using System;
using System.IO;
using TelcobrightFileOperations;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using LibraryExtensions.ConfigHelper;
using QuartzTelcobright;
using TelcobrightMediation;
using TelcobrightMediation.Config;
using FlexValidation;
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
            this.Tbc = new TelcobrightConfig(thisServerId);
        }

        public TelcobrightConfig GenerateConfig(DatabaseSetting schedulerDatabaseSetting)
        {
            if (string.IsNullOrWhiteSpace(this.OperatorName))
                throw new Exception("Operator name not configured in Config Generator");

            CdrSetting tempCdrSetting = new CdrSetting();//helps with getting some values initialized in constructors
            this.Tbc.CdrSetting = new CdrSetting()
            {
                SummaryTimeField = SummaryTimeFieldEnum.AnswerTime,
                PartialCdrEnabledNeIds =new List<int>() {7},
                DescendingOrderWhileListingFiles = false,
                DescendingOrderWhileProcessingListedFiles = false
            };
            this.Tbc.CdrSetting.ValidationRulesForInconsistentCdrs = new Dictionary<string, string>()
            {
                { "!String.IsNullOrEmpty(obj[98]) and !String.IsNullOrWhiteSpace(obj[98])",
                    "UniqueBillId cannot be empty" },//public const int Uniquebillid = 98;
                { $@"Validator.BooleanParsers['isNumericChecker'].Invoke(obj[2]) == true 
                         and Validator.IntParsers['intConverterProxy'].Invoke(obj[2]) > 0",
                    "SequenceNumber must be numeric and > 0" },//public const int Sequencenumber = 2;
                {"!String.IsNullOrEmpty(obj[5]) and !String.IsNullOrWhiteSpace(obj[5])",
                    "incomingroute cannot be empty" },//public const int Incomingroute = 5;
                {"!String.IsNullOrEmpty(obj[9]) and !String.IsNullOrWhiteSpace(obj[9])",
                    "OriginatingCalledNumber cannot be empty" },//public const int Originatingcallednumber = 9;
                {$@"Validator.BooleanParsers['isNumericChecker'].Invoke(obj[14]) 
                        and Validator.DoubleParsers['doubleConverterProxy'].Invoke(obj[14]) >= 0",
                    "durationsec must be numeric and >= 0" },//public const int Durationsec = 14;
                {$@"Validator.BooleanParsers['isDateTimeChecker'].Invoke(obj[29]) == true
                        and Validator.DateParsers['strToMySqlDtConverter'].Invoke(obj[29]) 
                        > date('"+ tempCdrSetting.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd") +"')",
                    "StartTime must be a valid datetime and > "+tempCdrSetting.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd") },//public const int Starttime = 29;
                {$@"Validator.BooleanParsers['isDateTimeChecker'].Invoke(obj[15]) == true and 
                        Validator.DateParsers['strToMySqlDtConverter'].Invoke(obj[15]) >= 
                        Validator.DateParsers['strToMySqlDtConverter'].Invoke(obj[29]"+")",
                    "EndTime must be a valid datetime and >= StartTime" },//public const int Endtime = 15;
                {"obj[54] == '1'",
                    "validflag must be 1" },//public const int Validflag = 54;
            };
            this.Tbc.CdrSetting.CommonMediationChecklist = new Dictionary<string, string>()
            {
                { "!String.IsNullOrEmpty(obj.UniqueBillId) and !String.IsNullOrWhiteSpace(obj.UniqueBillId)",
                    "UniqueBillId cannot be empty" },//public const int Uniquebillid = 98;
                { "obj.SequenceNumber > 0",
                    "SequenceNumber must be > 0" },//public const int Sequencenumber = 2;
                {"!String.IsNullOrEmpty(obj.incomingroute) and !String.IsNullOrWhiteSpace(obj.incomingroute)",
                    "incomingroute cannot be empty" },//public const int Incomingroute = 5;
                {"!String.IsNullOrEmpty(obj.OriginatingCalledNumber) and !String.IsNullOrWhiteSpace(obj.OriginatingCalledNumber)",
                    "OriginatingCalledNumber cannot be empty" },//public const int Originatingcallednumber = 9;
                {"obj.DurationSec >= 0",
                    "durationsec must be >= 0" },//public const int Durationsec = 14;
                {"obj.StartTime > date('"+ tempCdrSetting.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd") +"')",
                    "StartTime must be > "+tempCdrSetting.NotAllowedCallDateTimeBefore.ToString("yyyy-MM-dd") },//public const int Starttime = 29;
                {"obj.EndTime >= obj.StartTime",
                    "EndTime must be >= StartTime" },//public const int Endtime = 15;
                {"obj.validflag > 0",
                    "validflag must be > 0" },//public const int Validflag = 54;
                { "obj.SwitchId > 0",
                    "SwitchId must be > 0" },
                {"obj.idcall > 0",
                    "idcall must be > 0" },
                {"!String.IsNullOrEmpty(obj.FileName) and !String.IsNullOrWhiteSpace(obj.FileName)",
                    "FileName cannot be empty" },
                {"obj.finalrecord == 1",
                    "FinalRecord must be 1" },
                { "obj.EndTime !=null and obj.EndTime >= obj.StartTime",
                    "EndTime must be >= StartTime" },
                {"obj.durationsec > 0?obj.ChargingStatus == 1: obj.ChargingStatus == 0",
                    "ChargingStatus must be 1 when durationsec > 0 , otherwise == 0 " },
                { "obj.inPartnerId > 0",
                    "inPartnerId must be > 0" },
                { "obj.CallDirection > 0",
                    "CallDirection must be > 0" },
            };
            this.Tbc.CdrSetting.ServiceGroupConfigurations = new Dictionary<int, ServiceGroupConfiguration>();
            this.Tbc.CdrSetting.ServiceGroupConfigurations.Add(1, new ServiceGroupConfiguration(enabled: true)//domestic 
            {
                PartnerRules = new List<string>() { "PrtInByTg", "PrtOutByTg" },
                Ratingtrules = new List<RatingRule>()
                {
                    new RatingRule() {IdServiceFamily = ServiceFamilyType.A2Z,AssignDirection = 1},
                },
                MediationChecklistForAnsweredCdrs =
                    new Dictionary<string, string>()
                    {
                        {"obj.DurationSec >= 0",
                            "DurationSec must be >=  0" },
                        {"!String.IsNullOrEmpty(obj.outgoingroute) and !String.IsNullOrWhiteSpace(obj.outgoingroute)",
                            "outgoingroute cannot be empty" },
                        {"obj.outPartnerId > 0",
                            "outPartnerId must be > 0" },
                        {"!String.IsNullOrEmpty(obj.matchedprefixcustomer) and !String.IsNullOrWhiteSpace(obj.matchedprefixcustomer)",
                            "matchedprefixcustomer cannot be empty" },
                        {"obj.durationsec > 0 ? obj.CustomerCost > 0 : obj.CustomerCost == 0 ",
                            "CustomerCost must be > 0 when durationsec > 0  , otherwise == 0 " },
                        {"obj.durationsec > 0?obj.roundedduration > 0: obj.roundedduration >= 0",
                            "roundedduration must be > 0 when durationsec >0 , otherwise >= 0 " },
                    },
            });
            this.Tbc.CdrSetting.ServiceGroupConfigurations.Add(3, value: new ServiceGroupConfiguration(enabled:true)//intlInIcx
            {
                PartnerRules = new List<string>() { "PrtInByTg", "PrtOutByTg" },
                Ratingtrules = new List<RatingRule>()
                {
                    new RatingRule() {IdServiceFamily =ServiceFamilyType.A2Z,AssignDirection = 1},
                },
                MediationChecklistForAnsweredCdrs =
                    new Dictionary<string, string>()
                    {
                        {"obj.DurationSec >= 0",
                            "DurationSec must be >=  0" },
                        {"!String.IsNullOrEmpty(obj.outgoingroute) and !String.IsNullOrWhiteSpace(obj.outgoingroute)",
                            "outgoingroute cannot be empty" },
                        {"obj.outPartnerId > 0",
                            "outPartnerId must be > 0" },
                        {"!String.IsNullOrEmpty(obj.matchedprefixcustomer) and !String.IsNullOrWhiteSpace(obj.matchedprefixcustomer)",
                            "matchedprefixcustomer cannot be empty" },
                        {"obj.durationsec > 0 ? obj.CustomerCost > 0 : obj.CustomerCost == 0 ",
                            "CustomerCost must be > 0 when durationsec > 0  , otherwise == 0 " },
                        {"obj.durationsec > 0?obj.roundedduration > 0: obj.roundedduration >= 0",
                            "roundedduration must be > 0 when durationsec >0 , otherwise >= 0 " },
                    },
            });
            this.Tbc.CdrSetting.ServiceGroupConfigurations.Add(
                2, value: new ServiceGroupConfiguration(enabled:true)//intlOutIgw
            {
                PartnerRules = new List<string>() { "PrtInByTg", "PrtOutByTg" },
                Ratingtrules = new List<RatingRule>()
                {
                    new RatingRule() {IdServiceFamily = ServiceFamilyType.XyzIcx, AssignDirection = 0}
                },
                MediationChecklistForAnsweredCdrs = 
                    new Dictionary<string, string>()
                    {
                        {"obj.DurationSec >= 0",
                            "DurationSec must be >=  0" },
                        {"!String.IsNullOrEmpty(obj.CountryCode) and !String.IsNullOrWhiteSpace(obj.CountryCode)",
                            "CountryCode cannot be empty" },
                        {"!String.IsNullOrEmpty(obj.outgoingroute) and !String.IsNullOrWhiteSpace(obj.outgoingroute)",
                            "outgoingroute cannot be empty" },
                        {"obj.outPartnerId > 0",
                            "outPartnerId must be > 0" },
                        {"!String.IsNullOrEmpty(obj.MatchedPrefixY) and !String.IsNullOrWhiteSpace(obj.MatchedPrefixY)",
                            "MatchedPrefixY cannot be empty" },
                        //{"obj.durationsec > 0 ? obj.RevenueIGWOut > 0 : obj.RevenueIGWOut == 0 ",
                        //    "RevenueIGWOut must be > 0 when durationsec > 0 , otherwise == 0" },
                        {"obj.durationsec > 0 ? obj.SubscriberChargeXOut > 0 : obj.SubscriberChargeXOut == 0 ",
                            "SubscriberChargeXOut must be > 0 when durationsec > 0 , otherwise == 0" },
                        {"obj.durationsec > 0 ? obj.CarrierCostYIGWOut > 0 : obj.CarrierCostYIGWOut == 0 ",
                            "CarrierCostYIGWOut must be > 0 when durationsec > 0 , otherwise == 0" },
                        {"obj.PartialFlag >= 0",
                            "PartialFlag must be >=  0" },
                        {"obj.field1 >= 0",
                            "field1 must be >=  0" },
                        {"obj.field2 >= 0",
                            "field2 must be >=  0" },
                        {"obj.durationsec > 0 ? obj.roundedduration > 0 : obj.roundedduration == 0 ",
                            "roundedduration must be > 0 when durationsec > 0 , otherwise == 0" },
                    },
            });
            
            
            //write all configuration first in ws_topshelf / bin/debug, for production
            //also, in tester/bin/debug

            //temp use of ne, remove later


            //database part
            DirectorySettings directorySetting = new DirectorySettings("Directory Settings")
            {
                ApplicationRootDirectory = "c:/Telcobright"
            };
            this.Tbc.DirectorySettings = directorySetting;

            //***FILE LOCATIONS**********************************************
            //local/vault1: all app servers will use same local file location
            //the object "vault" will have a copy of below object for each app servers with server id as key and location as dictionary value
            FileLocation vaultJslZteDhk = new FileLocation()
            {
                Name = "Vault.JslZteDhk",//this is refered in ne table, name MUST start with "Vault"
                LocationType = "vault",//locationtype always lowercase
                OsType = "windows",
                PathSeparator = @"\",
                ServerIp = "",
                StartingPath = "C:/telcobright/Vault/Resources/CDR/JSL/JslZteDhk",
                User = "",
                Pass = "",
            };
            FileLocation appServerFtp1 = new FileLocation()
            {
                Name = "AppServerFTP1",//
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "192.168.2.216",
                StartingPath = "Resources/CDR/JSL/JslZteDhk",
                User = "ftpuser",
                Pass = "Takay1takaane",
            };
            FileLocation appServerFtp2 = new FileLocation()
            {
                Name = "AppServerFTP2",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = "/",
                ServerIp = "192.168.2.224",
                StartingPath = "Resources/CDR/JSL/JslZteDhk",
                User = "ftpuser",
                Pass = "Takay1takaane",
                Skip = true
            };
            //VAULT PART
            List<FileLocation> ftpLocations = new List<FileLocation>();
            ftpLocations.Add(appServerFtp1);
            ftpLocations.Add(appServerFtp2);
            Vault JslZteDhkvault = new Vault("Vault.JslZteDhk", this.Tbc, ftpLocations);
            JslZteDhkvault.LocalLocation = new SyncLocation(vaultJslZteDhk.Name) { FileLocation = vaultJslZteDhk };//don't pass this to constructor and set there, causes problem in json serialize
            this.Tbc.Vaults.Add(JslZteDhkvault);
            FileLocation JslZteDhk = new FileLocation()
            {
                Name = "JslZteDhk",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = "/home/zxss10_bsvr/data/bfile/bill/zsmart_media_bak",
                Sftphostkey = "",
                ServerIp = "10.133.34.12",
                User = "icxbill",
                Pass = "icx123",
                ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
                IgnoreZeroLenghFile = 1
            };

            //FileLocation S3_2 = new FileLocation()
            //{
            //    Name = "S3_2",
            //    locationType = "ftp",
            //    OSType = "windows",
            //    pathSeparator = "/",
            //    startingPath = "/oldcdr",
            //    sftphostkey = "ssh-rsa 1024 0d:fd:ac:4a:67:05:e9:76:ef:a0:d2:c0:f9:1a:55:c1",
            //    server = "172.16.16.242",
            //    user = "ftpuser",
            //    pass = "Takay1takaane",
            //    ExcludeBefore = new DateTime(2015, 6, 26, 0, 0, 0),
            //    IgnoreZeroLenghFile = 1
            //};

            FileLocation fileArchive1 = new FileLocation()//raw cdr archive
            {
                Name = "FileArchive1",
                LocationType = "ftp",
                OsType = "windows",
                PathSeparator = @"/",//backslash didn't work with winscp
                StartingPath = @"/archive",
                ServerIp = "10.100.201.20",
                User = "cdr",
                Pass = "cdr13531",
                IgnoreZeroLenghFile = 1
            };
            FileLocation fileArchiveCAS = new FileLocation()//raw cdr archive
            {
                Name = "FileArchiveCAS",
                LocationType = "ftp",
                OsType = "linux",
                PathSeparator = "/",
                StartingPath = @"/",
                ServerIp = "192.168.100.83", //server = "172.16.16.242",
                User = "adminjibon",
                Pass = "jibondhara35#",
                IgnoreZeroLenghFile = 1,
            };
            //add locations to directory settings
            this.Tbc.DirectorySettings.FileLocations.Add(vaultJslZteDhk.Name, vaultJslZteDhk);
            this.Tbc.DirectorySettings.FileLocations.Add(appServerFtp1.Name, appServerFtp1);
            this.Tbc.DirectorySettings.FileLocations.Add(appServerFtp2.Name, appServerFtp2);
            this.Tbc.DirectorySettings.FileLocations.Add(JslZteDhk.Name, JslZteDhk);

            this.Tbc.DirectorySettings.FileLocations.Add(fileArchive1.Name, fileArchive1);
            this.Tbc.DirectorySettings.FileLocations.Add(fileArchiveCAS.Name, fileArchiveCAS);


            //sync pair platinum:Vault
            SyncPair JslZteDhkVault = new SyncPair("JslZteDhk:Vault")
            {
                SkipSourceFileListing = false,
                SrcSyncLocation = new SyncLocation("JslZteDhk")
                {
                    FileLocation = JslZteDhk,
                    DescendingFileListByFileName = this.Tbc.CdrSetting.DescendingOrderWhileListingFiles
                },
                DstSyncLocation = new SyncLocation("Vault_JslZteDhk")
                {
                    FileLocation = vaultJslZteDhk
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
                    ExpFileNameFilter = new SpringExpression(@"Name.StartsWith('ICX')
                                                                and
                                                                (Name.EndsWith('.DAT'))
                                                                and Length>0")
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",//make sure when copying to vault always .tmp ext used
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(@"Name.Insert(0,'')"),
                    CompressionType = CompressionType.None
                }
            };


            //sync pair Vault_S3:FileArchive1
            SyncPair vaultS3FileArchive1 = new SyncPair("Vault:FileArchive1")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_JslZteDhk")
                {
                    FileLocation = vaultJslZteDhk
                },
                DstSyncLocation = new SyncLocation("FileArchive1")
                {
                    FileLocation = fileArchive1
                },
                SrcSettings = new SyncSettingsSource()
                {
                    SecondaryDirectory = "Downloaded",
                    ExpFileNameFilter = null,
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",
                    Overwrite = true,
                    CompressionType = CompressionType.Sevenzip,
                    SubDirRule = new SyncSettingsDstSubDirectoryRule
                    (
                        DateWiseSubDirCreationType.ByFileName,
                        new SpringExpression(@"Name.Substring(16,8)"), //"S3_2_" is appended at vault
                        "yyyyMMdd",
                        true
                    )
                }
            };
            //sync pair vault:CAS
            SyncPair vaultS3CAS = new SyncPair("Vault:CAS")
            {
                SkipCopyingToDestination = false,
                SkipSourceFileListing = true,
                SrcSyncLocation = new SyncLocation("Vault_JslZteDhk")
                {
                    FileLocation = vaultJslZteDhk
                },
                DstSyncLocation = new SyncLocation("CAS")
                {
                    FileLocation = fileArchiveCAS
                },
                SrcSettings = new SyncSettingsSource()
                {
                    ExpFileNameFilter = null,//source filter not required, job created after newcdr for this pair
                },
                DstSettings = new SyncSettingsDest()
                {
                    FileExtensionForSafeCopyWithTempFile = ".tmp",
                    Overwrite = true,
                    ExpDestFileName = new SpringExpression(""),
                    CompressionType = CompressionType.None,
                    SubDirRule = null
                }
            };

            //add sync pairs to directory config
            directorySetting.SyncPairs.Add(JslZteDhkVault.Name, JslZteDhkVault);
            directorySetting.SyncPairs.Add(vaultS3FileArchive1.Name, vaultS3FileArchive1);
            directorySetting.SyncPairs.Add(vaultS3CAS.Name, vaultS3CAS);
            //load the syncpairs in dictioinary, first by source
            foreach (SyncPair sp in directorySetting.SyncPairs.Values)
            {
                if (directorySetting.SyncLocations.ContainsKey(sp.SrcSyncLocation.Name) == false)
                {
                    directorySetting.SyncLocations.Add(sp.SrcSyncLocation.Name, sp.SrcSyncLocation);
                }
            }
            foreach (SyncPair sp in directorySetting.SyncPairs.Values)
            {
                if (directorySetting.SyncLocations.ContainsKey(sp.DstSyncLocation.Name) == false)
                {
                    directorySetting.SyncLocations.Add(sp.DstSyncLocation.Name, sp.DstSyncLocation);
                }
            }
            //add archive locations to CdrSettings
            this.Tbc.CdrSetting.BackupSyncPairNames = new List<string>();
            this.Tbc.CdrSetting.BackupSyncPairNames.Add(vaultS3FileArchive1.Name);
            this.Tbc.CdrSetting.BackupSyncPairNames.Add(vaultS3CAS.Name);


            //configuration for server1
            ApplicationServerConfig serverConfig1 = new ApplicationServerConfig(this.Tbc) { ServerId = 1, OwnIpAddress = "192.168.101.1" };
            //configuration for server2
            ApplicationServerConfig serverConfig2 = new ApplicationServerConfig(this.Tbc) { ServerId = 2 };

            this.Tbc.ApplicationServersConfig.Add(serverConfig1.ServerId.ToString(), serverConfig1);
            this.Tbc.ApplicationServersConfig.Add(serverConfig2.ServerId.ToString(), serverConfig1);

            DatabaseSetting databaseSetting = schedulerDatabaseSetting.GetCopy();
            databaseSetting.DatabaseName = this.OperatorName;//change dbname here if required
            this.Tbc.DatabaseSetting = databaseSetting;

           PortalSettings portalSetting = new PortalSettings("Portal Settings")
            {
               HomePageUrl = "~/Dashboard.aspx",
               AlternateDisplayName = "ICX Manager",
               PortalSites = new List<InternetSite>()
                {
                    new InternetSite(this.Tbc)//make sure that first one always the http portal
                    {
                        SiteType = "http",
                        SiteName = databaseSetting.DatabaseName,
                        SiteId = 1,
                        PhysicalPath = "C:/inetpub/wwwroot/" + this.Tbc.DatabaseSetting.DatabaseName,
                        BindAddress = this.Tbc.DirectorySettings.FileLocations["AppServerFTP" + this.Tbc.ServerId].ServerIp + ":80",
                        TemplateFileName = "../../" + this.Tbc.DatabaseSetting.DatabaseName + "/tmplPortalWebSite.txt",
                        ApplicationPool=new IisApplicationPool()
                        {
                            AppPoolName = this.Tbc.DatabaseSetting.DatabaseName,
                            TemplateFileName = "../../" + this.Tbc.DatabaseSetting.DatabaseName + "/tmplPortalAppPools.txt",
                        },
                        ImpersonateUserName="Mustafa",
                        ImpersonatePassword="Habib321"
                    },
                    new InternetSite(this.Tbc)
                    {
                        SiteType = "ftp",
                        SiteName = databaseSetting.DatabaseName,
                        SiteId = 2,
                        PhysicalPath = "C:/sftp_root",
                        BindAddress = this.Tbc.DirectorySettings.FileLocations["AppServerFTP" + this.Tbc.ServerId].ServerIp + ":21",
                        TemplateFileName = "../../" + this.Tbc.DatabaseSetting.DatabaseName + "/tmplPortalFtpSite.txt"
                    }
                },
                DicConfigObjects = new Dictionary<string, object>()
                {
                    { "CdrFieldTemplate",new List<CdrFieldTemplate>()//the list is the object
                        {
                           new CdrFieldTemplate()
                           {
                               FieldTemplateName="Basic",
                               Fields=new List<string>()
                                    {
                                        "starttime as `Start Time`",
                                        "AnswerTime as `Answer Time`",
                                        "endtime as `End Time`",
                                        "inpartner.partnername as `In Partner`",
                                        "outpartner.partnername as `Out Partner`",
                                        "concat(switchid,'-',incomingroute) as `Ingress Route`",
                                        "concat(switchid,'-',outgoingroute)  as `Egress Route`",
                                        "OriginatingCallingNumber as `Ingress Calling Number`",
                                        "OriginatingCalledNumber as `Ingress Called Number`",
                                        "TerminatingCallingNumber as `Egress Calling Number`",
                                        "TerminatingCalledNumber as `Egress Called Number`",
                                        "durationsec as `Actual Duration`",
                                        "roundedduration as `Rounded Duration`",
                                        "duration1 as Duration1",
                                        "duration2 as Duration2",
                                        "Duration3 as Duration3",
                                        "releasecauseingress as `Ingress CauseCode`",
                                    }
                           },
                           new CdrFieldTemplate()
                           {
                               FieldTemplateName="All",
                               Fields=new List<string>()
                                    {
                                        "*"
                                    }
                           },
                           new CdrFieldTemplate()
                           {
                                FieldTemplateName="Basic_Error",
                                Fields=new List<string>()
                                    {
                                        "c.field4 as `Error Reason`",
                                        "starttime as `Start Time`",
                                        "AnswerTime as `Answer Time`",
                                        "endtime as `End Time`",
                                        "c.inPartnerId",
                                        "c.outPartnerId",
                                        "concat(switchid,'-',incomingroute) as `Ingress Route`",
                                        "concat(switchid,'-',outgoingroute)  as `Egress Route`",
                                        "OriginatingCallingNumber as `Ingress Calling Number`",
                                        "OriginatingCalledNumber as `Ingress Called Number`",
                                        "TerminatingCallingNumber as `Egress Calling Number`",
                                        "TerminatingCalledNumber as `Egress Called Number`",
                                        "durationsec as `Actual Duration`",
                                        "roundedduration as `Rounded Duration`",
                                        "duration1 as Duration1",
                                        "duration2 as Duration2",
                                        "Duration3 as Duration3",
                                        "releasecauseingress as `Ingress CauseCode`",
                                        "ReleaseCauseEgress as `Egress Cause Code`"
                                    }
                           } ,
                           new CdrFieldTemplate()
                           {
                               FieldTemplateName="All_Error",
                               Fields=new List<string>()
                                   {
                                       "c.field4 as `Error Reason`",
                                       "c.*"
                                   }
                           }
                        }//list of cdr templates
                    }//one config object
                }
            };//portalSettings

            //settings for report pages, same settings to be copied...
            List<SettingByRoles> settingIntlIn = new List<SettingByRoles>()
            {
                {
                    new SettingByRoles()
                    {
                        RoleNames = new List<string>()
                        {
                            "admin","billing"
                        },
                        SpringExpressionIfRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=true",
                            "CheckBoxShowCost.Checked=true",
                            "GridView1.Columns[7].Visible=true",//actual duration
                            "GridView1.Columns[9].Visible=true",//duration1
                            "GridView1.Columns[6].Visible=true",//Connect count
                            "GridView1.Columns[19].Visible=true",//CCR
                            "GridView1.Columns[20].Visible=false",//Connect count by cc
                            "GridView1.Columns[21].Visible=false",//CCR by cc
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[6].Visible=true",//Connect count
                            "GridView1.Columns[7].Visible=false",
                            "GridView1.Columns[9].Visible=false",
                            "GridView1.Columns[8].HeaderText=Duration",
                            "GridView1.Columns[19].Visible=true",//CCR
                            "GridView1.Columns[20].Visible=false",//Connect count by cc
                            "GridView1.Columns[21].Visible=false",//CCR by cc
                        }
                    }
                }
            };//settings for one role within a page
            List<SettingByRoles> settingIntlInRoute = new List<SettingByRoles>()
            {
                {
                    new SettingByRoles()
                    {
                        RoleNames = new List<string>()
                        {
                            "admin","billing"
                        },
                        SpringExpressionIfRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=true",
                            "GridView1.Columns[6].Visible=true",//connect count
                            "GridView1.Columns[8].Visible=true",//duration1
                            "GridView1.Columns[19].Visible=true",//CCR
                            "GridView1.Columns[20].Visible=false",//connect by cc
                            "GridView1.Columns[21].Visible=false",//CCR by cc
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[6].Visible=true",
                            "GridView1.Columns[8].Visible=false",
                            "GridView1.Columns[7].HeaderText=Duration",
                            "GridView1.Columns[19].Visible=true",//CCR
                            "GridView1.Columns[20].Visible=false",//connect by cc
                            "GridView1.Columns[21].Visible=false",//CCR by cc
                        }
                    }
                }
            };//settings for one role within a page
            List<SettingByRoles> settingIntlOut = new List<SettingByRoles>()
            {
                {
                    new SettingByRoles()
                    {
                        RoleNames = new List<string>()
                        {
                            "admin","billing"
                        },
                        SpringExpressionIfRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=true",
                            "GridView1.Columns[8].Visible=true",//connected calls
                            "GridView1.Columns[9].Visible=true",//actual duration
                            "GridView1.Columns[12].Visible=true",//supplier duration
                            "GridView1.Columns[16].Visible=true",//CCR
                            "GridView1.Columns[17].Visible=false",//connect by cc
                            "GridView1.Columns[18].Visible=false",//CCR by cc
                        },
                        SpringExpressionIfNotRole = new List<string>()
                        {
                            "CheckBoxShowCost.Enabled=false",
                            "GridView1.Columns[8].Visible=true",//connected calls
                            "GridView1.Columns[9].Visible=false",//actual duration
                            "GridView1.Columns[12].Visible=false",//supplier duration
                            "GridView1.Columns[16].Visible=true",//CCR
                            "GridView1.Columns[17].Visible=false",//connect by cc
                            "GridView1.Columns[18].Visible=false",//CCR by cc
                        }
                    }
                }
            };//settings for one role within a page
            portalSetting.PageSettings = new PortalPageSettings()
            {
                DicPageSettingsByRole = new Dictionary<string, List<SettingByRoles>>()
                {
                    { "~/site.master",//settings for master page, //use tolower for master because got different results during execution
                        new List<SettingByRoles>()
                        {
                            new SettingByRoles()
                           {
                               RoleNames = new List<string>()
                               {
                                    "admin"
                               },
                               SpringExpressionIfRole = new List<string>()
                                       {
                                            "nodes['Configuration'].Expanded=true",
                                            "nodes['Mediation'].Expanded=true",
                                            "nodes['Settings'].Expanded=true",
                                            "nodes['Billing Reports'].Expanded=true"
                                       },
                               SpringExpressionIfNotRole = new List<string>()
                                       {
                                            "nodes['Configuration'].Expanded=false",
                                            "nodes['Mediation'].Expanded=false",
                                            "nodes['Settings'].Expanded=false",
                                            "nodes['Billing Reports'].Expanded=false"
                                        }
                           },
                            new SettingByRoles()
                            {
                                RoleNames = new List<string>()
                                {
                                    "billing"
                                },
                                SpringExpressionIfRole = new List<string>()
                                {
                                    "nodes['Configuration'].Expanded=true",
                                    "nodes['Mediation'].Expanded=true",
                                    "nodes['Settings'].Expanded=true",
                                    "nodes['Billing Reports'].Expanded=true"
                                },
                                SpringExpressionIfNotRole = new List<string>()
                                {
                                    
                                }
                            }

                        }//list of settings by Roles
                    },
                    { "~/reports/InternationalIn.aspx",//settings for report pages
                        settingIntlIn
                    },
                    { "~/reports/InternationalOut.aspx",
                        settingIntlOut
                    },
                    { "~/reports/RouteInternationalIn.aspx",
                        settingIntlInRoute
                    },
                    { "~/reports/icx/InternationalIn_ICX.aspx",//settings for report pages
                        settingIntlIn
                    },
                    { "~/reports/icx/InternationalOut _ICX.aspx",
                        settingIntlOut
                    },
                    { "~/reports/icx/Domestic.aspx",
                        settingIntlIn
                    }
                },//dictionary of page settings

            };
            this.Tbc.PortalSettings = portalSetting;
            return this.Tbc;
        }
    }
}
