using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itenso.TimePeriod;
namespace LibraryExtensions.ConfigHelper
{
    public class DatabaseSetting
    {
        public String ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string AdminUserName { get; set; }
        public string AdminPassword { get; set; }
        public string ReadOnlyUserName { get; set; }
        public string ReadOnlyPassword { get; set; }
        public string DatabaseEngine { get; set; }
        public List<string> DateWisePartitionedTablesWithPartitionColName { get; set; }
        public DateTime PartitionStartDate { get; set; }
        public readonly int NoOfPartitions = 1024;

        public DatabaseSetting()
        {
            this.DatabaseEngine = "innodb";
            this.PartitionStartDate= DateTime.Now.Date.AddYears(-1);
            this.DateWisePartitionedTablesWithPartitionColName=new List<string>()
            {
                "acc_chargeable/transactiontime",
                "acc_ledger/transactiontime",
                "acc_ledger_summary/transactiondate",
                "acc_temp_transaction/transactionTime",
                "acc_transaction/transactionTime",
                "cdr/starttime",
                "cdrpartiallastaggregatedrawinstance/starttime",
                "cdrpartialrawinstance/starttime",
                "cdrpartialreference/CallDate",
                "sum_voice_day_01/tup_starttime",
                "sum_voice_day_02/tup_starttime",
                "sum_voice_day_03/tup_starttime",
                "sum_voice_hr_01/tup_starttime",
                "sum_voice_hr_02/tup_starttime",
                "sum_voice_hr_03/tup_starttime"
            };
        }

        public DatabaseSetting GetCopy()
        {
            return new DatabaseSetting()
            {
                ServerName=this.ServerName,
                DatabaseName = this.DatabaseName,
                AdminUserName=this.AdminUserName,
                AdminPassword = this.AdminPassword,
                ReadOnlyUserName = this.ReadOnlyUserName,
                ReadOnlyPassword = this.ReadOnlyPassword
            };
        }
    }
}
