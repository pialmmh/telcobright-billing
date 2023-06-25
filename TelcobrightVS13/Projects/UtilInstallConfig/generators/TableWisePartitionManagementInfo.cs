using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
namespace InstallConfig
{
    public class TablePartitionManager
    {
        public MySqlConnection con;
        public string tableName { get; set; }
        public List<string> PartitionDropinfo = new List<string>();
        public List<PartitionInfo> PartitionCreateinfo = new List<PartitionInfo>();

        public TablePartitionManager(string conStr)
        {
            this.con = new MySqlConnection(conStr);
            this.con.Open();
        }
    }
}
