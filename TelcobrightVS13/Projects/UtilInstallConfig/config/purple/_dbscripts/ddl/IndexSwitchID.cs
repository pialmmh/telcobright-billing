using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TelcobrightInfra;
namespace InstallConfig
{
    [Export("Script", typeof(IScript))]
    //[ExportMetadata("Symbol", '+')]
    public class  IndexSwitchID : IScript
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "";
        public ScriptType ScriptType => ScriptType.SqlDDL;
        public string ScriptDir { get; set; }
        public string SrcTextFileName { get; set; } = "icxSp.sql";
        public string GetScript(object data)
        {
            MySqlConnection con = (MySqlConnection)data;
            string sql1 = $@"SELECT COUNT(1) indexExists FROM INFORMATION_SCHEMA.STATISTICS
                            WHERE table_schema=DATABASE() AND table_name='cdr' AND index_name='ind_switchid';";
            MySqlCommand cmd = new MySqlCommand(sql1, con);
            MySqlDataReader reader = cmd.ExecuteReader();
            string ans = "";
            while (reader.Read())
            {
                ans = reader[0].ToString();
            }
            reader.Close();

            string sql = $@"ALTER TABLE `cdr` 
                            ADD INDEX `ind_switchid` (`SwitchId` ASC);";
            if (ans == "1")
                return "";

            return sql;
        }
    }
}
