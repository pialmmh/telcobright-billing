using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;

namespace InstallConfig
{
    public class CasDockerDbHelper
    {
        public readonly static  Dictionary<string, string> IcxVsdbHostNames = new Dictionary<string, string>()
        {
            {"agni_cas","172.16.1.21"},
            {"banglatelecom_cas","172.16.1.21"},
            {"bantel_cas","172.16.1.21"},
            {"gazinetworks_cas","172.16.1.22"},
            {"summit_cas","172.16.1.22"},
            {"ringtech_cas","172.16.1.22"},
            {"mothertelecom_cas","172.16.1.23"},
            {"teleplusnewyork_cas","172.16.1.23"},
            {"voicetel_cas","172.16.1.23"},
            {"mnh_cas","172.16.1.24"},
            {"srtelecom_cas","172.16.1.24"},
            {"banglaicx_cas","172.16.1.25"},
            {"crossworld_cas","172.16.1.25"},
            {"mmcommunications_cas","172.16.1.26"},
            {"btcl_cas","172.16.1.26"},
            {"imamnetwork_cas","172.16.1.26"},
            {"jibondhara_cas","172.16.1.27"},
            {"paradise_cas","172.16.1.27"},
            {"purple_cas","172.16.1.27"},
            {"softex_cas","172.16.1.28"},
            {"sheba_cas","172.16.1.28"},
            {"teleexchange_cas","172.16.1.28"},
            {"newgenerationtelecom_cas","172.16.1.28"},
            {"smshub","localhost"},
            {"getco_cas","172.16.1.24"}
        };
        public DeploymentEnvironment DeploymentEnvironment { get; }
        public CasDockerDbHelper(DeploymentEnvironment deploymentEnvironment)
        {
            DeploymentEnvironment = deploymentEnvironment;
        }

        public class Db
        {
            public const string WriteUserNameForApplication = "fduser";
            public const string WritePasswordForApplication = "Takay1takaane$";
            public const string DatabaseEngine = "innodb";
            public const string StorageEngineForPartitionedTables = "innodb";
            public const string PartitionStartDate = "2023-06-01";
            public const int PartitionLenInDays = 3;
            public const string ReadOnlyUserNameForApplication = "dbreader";
            public const string ReadOnlyPasswordForApplication = "Takay1takaane$";
            public const bool UseVarcharInsteadOfTextForMemoryEngine = true;  //required for windows
        }
        public static DatabaseSetting getCommonDatabaseSetting(string databaseName)
        {
            return new DatabaseSetting()
            {
                ServerName = IcxVsdbHostNames[databaseName],
                DatabaseName = databaseName,
                WritePasswordForApplication = Db.WritePasswordForApplication,
                WriteUserNameForApplication = Db.WriteUserNameForApplication,
                DatabaseEngine = Db.DatabaseEngine,
                StorageEngineForPartitionedTables = Db.StorageEngineForPartitionedTables,
                PartitionStartDate = Db.PartitionStartDate.ConvertToDateTimeFromCustomFormat("yyyy-MM-dd"),
                PartitionLenInDays = Db.PartitionLenInDays,
                ReadOnlyUserNameForApplication = Db.ReadOnlyUserNameForApplication,
                ReadOnlyPasswordForApplication = Db.ReadOnlyPasswordForApplication,
                UseVarcharInsteadOfTextForMemoryEngine=Db.UseVarcharInsteadOfTextForMemoryEngine

            };
        }
        public DatabaseSetting getDatabaseSettingForMySqlWin(string databaseName)
        {
            DatabaseSetting databaseSetting = getCommonDatabaseSetting(databaseName);
            databaseSetting.DatabaseEngine = "innodb";
            databaseSetting.StorageEngineForPartitionedTables = "innodb";
            return databaseSetting;
        }
        public DatabaseSetting getDatabaseSettingForMySqlLinux(string databaseName)
        {
            DatabaseSetting databaseSetting = getCommonDatabaseSetting(databaseName);
            databaseSetting.DatabaseEngine = "innodb";
            databaseSetting.StorageEngineForPartitionedTables = "tokudb";
            return databaseSetting;
        }
    }
}
