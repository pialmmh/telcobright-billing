using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;

namespace InstallConfig.config._helper
{
    public class CasDatabaseHelper
    {
        static DatabaseSetting getDatabaseSetting(string databaseName)
        {
            return new DatabaseSetting()
            {
                ServerName = "localhost",
                DatabaseName = databaseName,
                AdminPassword = "Takay1#$ane",
                AdminUserName = "root",
                DatabaseEngine = "innodb",
                StorageEngineForPartitionedTables = "tokudb",
                PartitionStartDate = new DateTime(2023, 1, 1),
                PartitionLenInDays = 1,
                ReadOnlyUserName = "dbreader",
                ReadOnlyPassword = "Takay1takaane"
            };
        }
    }
}
