using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelcobrightInfra;
namespace InstallConfig
{
    [Export("Script", typeof(IScript))]
    //[ExportMetadata("Symbol", '+')]
    public class ServiceGroupNotNullCdrError : IScript
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "";
        public ScriptType ScriptType => ScriptType.SqlDDL;
        public string ScriptDir { get; set; }
        public string SrcTextFileName { get; set; }
        public string GetScript(object data)
        {
            //var map = (Dictionary<string, string>) data;
            //string operatorName = map["operatorName"];
            string sql = $@"ALTER TABLE `cdrerror` 
                        CHANGE COLUMN `ServiceGroup` `ServiceGroup` VARCHAR(100) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_bin' NULL ,
                        CHANGE COLUMN `DurationSec` `DurationSec` VARCHAR(100) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_bin' NULL ,
                        CHANGE COLUMN `EndTime` `EndTime` VARCHAR(100) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_bin' NULL ,
                        CHANGE COLUMN `StartTime` `StartTime` DATETIME NULL ,
                        CHANGE COLUMN `SignalingStartTime` `SignalingStartTime` VARCHAR(100) CHARACTER SET 'utf8mb4' COLLATE 'utf8mb4_bin' NULL ;
                        ";
            return sql;
        }
    }
}
