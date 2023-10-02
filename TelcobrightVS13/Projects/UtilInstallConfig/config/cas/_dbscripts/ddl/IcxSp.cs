using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightInfra;
namespace InstallConfig
{
    [Export("Script", typeof(IScript))]
    //[ExportMetadata("Symbol", '+')]
    public class IcxSp : IScript
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "";
        public ScriptType ScriptType => ScriptType.SqlDDL;
        public string ScriptDir { get; set; }
        public string SrcTextFileName { get; set; } = "icxSp.sql";
        public string GetScript(object data)
        {
            //var map = (Dictionary<string, string>) data;
            string rawSqlFileName = this.ScriptDir + Path.DirectorySeparatorChar + "sql"
                + Path.DirectorySeparatorChar + this.SrcTextFileName;
            string sql = File.ReadAllText(rawSqlFileName);
            return sql;
        }
    }
}
