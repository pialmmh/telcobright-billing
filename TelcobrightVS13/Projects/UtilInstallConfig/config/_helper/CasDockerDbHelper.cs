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
        private  static  Dictionary<string, string> IcxVsdbHostNames = new Dictionary<string, string>()
        {
            {"agni_cas","10.100.150.20"},
            {"banglatelecom_cas","10.100.150.20"},
            {"bantel_cas","10.100.150.20"},
            {"gazinetworks_cas","10.100.150.21"},
            {"summit_cas","10.100.150.21"},
            {"ringtech_cas","10.100.150.21"},
            {"mothertelecom_cas","10.100.150.22"},
            {"teleplusnewyork_cas","10.100.150.22"},
            {"voicetel_cas","10.100.150.22"},
            {"mnh_cas","10.100.150.23"},
            {"srtelecom_cas","10.100.150.23"},
            {"banglaicx_cas","10.100.150.24"},
            {"crossworld_cas","10.100.150.24"},
            {"btcl_cas","10.100.150.25"},
            {"imamnetwork_cas","10.100.150.25"},
            {"jibondhara_cas","10.100.150.26"},
            {"paradise_cas","10.100.150.26"},
            {"purple_cas","10.100.150.26"},
            {"softex_cas","10.100.150.27"},
            {"sheba_cas","10.100.150.27"},
            {"teleexchange_cas","10.100.150.27"},
            {"newgenerationtelecom_cas","10.100.150.27"}
        };
        public DeploymentEnvironment DeploymentEnvironment { get; }
        public CasDockerDbHelper(DeploymentEnvironment deploymentEnvironment)
        {
            DeploymentEnvironment = deploymentEnvironment;
        }

        public class Db
        {
            public const string AdminPassword = "Takay1takaane$";
            public const string AdminUserName = "fduser";
            public const string DatabaseEngine = "innodb";
            public const string StorageEngineForPartitionedTables = "innodb";
            public const string PartitionStartDate = "2023-06-01";
            public const int PartitionLenInDays = 3;
            public const string ReadOnlyUserName = "dbreader";
            public const string ReadOnlyPassword = "Takay1takaane$";
            public const bool UseVarcharInsteadOfTextForMemoryEngine = true;  //required for windows
        }
        public static DatabaseSetting getCommonDatabaseSetting(string databaseName)
        {
            return new DatabaseSetting()
            {
                ServerName = IcxVsdbHostNames[databaseName],
                DatabaseName = databaseName,
                AdminPassword = Db.AdminPassword,
                AdminUserName = Db.AdminUserName,
                DatabaseEngine = Db.DatabaseEngine,
                StorageEngineForPartitionedTables = Db.StorageEngineForPartitionedTables,
                PartitionStartDate = Db.PartitionStartDate.ConvertToDateTimeFromCustomFormat("yyyy-MM-dd"),
                PartitionLenInDays = Db.PartitionLenInDays,
                ReadOnlyUserName = Db.ReadOnlyUserName,
                ReadOnlyPassword = Db.ReadOnlyPassword,
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
