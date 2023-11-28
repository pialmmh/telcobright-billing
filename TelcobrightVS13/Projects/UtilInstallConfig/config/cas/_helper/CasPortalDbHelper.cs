using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;

namespace InstallConfig
{
    public class CasPortalDbHelper
    {
        public DeploymentEnvironment DeploymentEnvironment { get; }
        public CasPortalDbHelper(DeploymentEnvironment deploymentEnvironment)
        {
            DeploymentEnvironment = deploymentEnvironment;
        }

        public class Db
        {
            public const string ServerName = "172.16.1.5";
            public const string WritePasswordForApplication = "Takay1takaane$";
            public const string WriteUserNameForApplication = "fduser";
            public const string DatabaseEngine = "innodb";
            public const string StorageEngineForPartitionedTables = "innodb";
            public const string PartitionStartDate = "2023-06-01";
            public const int PartitionLenInDays = 3;
            public const string ReadOnlyUserNameForApplication = "dbreader";
            public const string ReadOnlyPasswordForApplication = "Takay1takaane";
            public const bool UseVarcharInsteadOfTextForMemoryEngine = true;  //required for windows
        }
        public static DatabaseSetting getCommonDatabaseSetting(string databaseName)
        {
            return new DatabaseSetting()
            {
                ServerName = Db.ServerName,
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
