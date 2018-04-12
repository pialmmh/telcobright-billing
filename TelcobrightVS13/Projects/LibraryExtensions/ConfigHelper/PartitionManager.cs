using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace LibraryExtensions.ConfigHelper
{
    public class PartitionManager
    {
        DatabaseSetting DatabaseSetting { get; set; }
        MySqlConnection Con { get; set; }

        public PartitionManager(DatabaseSetting databaseSetting)
        {
            this.DatabaseSetting = databaseSetting;
            this.Con =
                new MySqlConnection(
                    $@"server={this.DatabaseSetting.ServerName};
                       User Id={this.DatabaseSetting.AdminUserName};
                       password={
                            this.DatabaseSetting.AdminPassword
                        };Persist Security Info=True;default command timeout=3600;
                       database={this.DatabaseSetting.DatabaseName}");
            this.Con.Open();
        }

        public void ResetPartitions()
        {
            foreach (string dateWisePartitionedTableName in this.DatabaseSetting.DateWisePartitionedTablesWithPartitionColName)
            {
                PartitionManagerPerTable partitionManagerPerTable =
                    new PartitionManagerPerTable(dateWisePartitionedTableName, this.DatabaseSetting.PartitionStartDate,
                        this.DatabaseSetting.NoOfPartitions, this.DatabaseSetting.DatabaseEngine, this.Con);
                partitionManagerPerTable.ResetPartitions();
            }
        }

    }
}


