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
    public class FkIdPartnerRouteToBtrc : IScript
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "";
        public ScriptType ScriptType => ScriptType.SqlDDL;
        public string GetScript(object data)
        {
            //var map = (Dictionary<string, string>) data;
            //string operatorName = map["operatorName"];
            string sql = $@"ALTER TABLE route DROP FOREIGN KEY route_ibfk_1;
            ALTER TABLE route ADD INDEX fk_partner_idx (idPartner ASC), DROP INDEX idCarrier ;
            ALTER TABLE route ADD CONSTRAINT fk_partner FOREIGN KEY (idPartner) REFERENCES btrc_cas.partner (idPartner) ON DELETE NO ACTION ON UPDATE NO ACTION; ";
            return sql;
        }
    }
}
