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
        public string DatabaseEngine { get; set; } = "innodb";
        public List<string> DateWisePartitionedTablesWithPartitionColName { get; set; }
        public readonly int MaxPartitionsPerTable = 1024;
        public DateTime PartitionStartDate { get; set; } = new DateTime(2000, 1, 1);
        public string StorageEngineForPartitionedTables { get; set; } = "innodb";
        public int PartitionLenInDays { get; set; } = 1;
        public List<string> PartitionedTables { get; }
        public string CharacterSet { get; set; } = "utf8mb4";
        public string Collate { get; set; } = "utf8mb4_bin";
        public bool UseVarcharInsteadOfTextForMemoryEngine { get; set; } = false;
        public DatabaseSetting()
        {
            this.PartitionedTables = new List<string>
            {
                "acc_chargeable",
                "acc_ledger_summary",
                "acc_ledger_summary_billed",
                "acc_transaction",
                "cdr",
                "cdrerror",
                "cdrpartiallastaggregatedrawinstance",
                "cdrpartialrawinstance",
                "cdrpartialreference",
                "sum_voice_day_01",
                "sum_voice_day_02",
                "sum_voice_day_03",
                "sum_voice_day_04",
                "sum_voice_day_05",
                "sum_voice_day_06",
                "sum_voice_hr_01",
                "sum_voice_hr_02",
                "sum_voice_hr_03",
                "sum_voice_hr_04",
                "sum_voice_hr_05",
                "sum_voice_hr_06",
            };
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
            var databasettings= (DatabaseSetting)this.MemberwiseClone();
            return databasettings;
        }
    }
}
