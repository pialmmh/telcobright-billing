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
            string sql = $@"ALTER TABLE `cdr` 
                            ADD INDEX `ind_switchid` (`SwitchId` ASC);";
            return sql;
        }
    }
}
