using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;

namespace InstallConfig
{
    public class CasConfigHelper
    {
        public DeploymentEnvironment DeploymentEnvironment { get; }


        public CasConfigHelper(DeploymentEnvironment deploymentEnvironment)
        {
            DeploymentEnvironment = deploymentEnvironment;
        }

        public class Db
        {
            public const string ServerName = "localhost";
            public const string AdminPassword = "Takay1takaane$";
            public const string AdminUserName = "fduser";
            public const string DatabaseEngine = "innodb";
            public const string StorageEngineForPartitionedTables = "innodb";
            public const string PartitionStartDate = "2023-06-01";
            public const int PartitionLenInDays = 3;
            public const string ReadOnlyUserName = "dbreader";
            public const string ReadOnlyPassword = "Takay1takaane";
        }
        private DatabaseSetting getCommonDatabaseSetting(string databaseName)
        {
            return new DatabaseSetting()
            {
                ServerName = Db.ServerName,
                DatabaseName = databaseName,
                AdminPassword = Db.AdminPassword,
                AdminUserName = Db.AdminUserName,
                DatabaseEngine = Db.DatabaseEngine,
                StorageEngineForPartitionedTables = Db.StorageEngineForPartitionedTables,
                PartitionStartDate = Db.PartitionStartDate.ConvertToDateTimeFromCustomFormat("yyyy-MM-dd"),
                PartitionLenInDays = Db.PartitionLenInDays,
                ReadOnlyUserName = Db.ReadOnlyUserName,
                ReadOnlyPassword = Db.ReadOnlyPassword
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
