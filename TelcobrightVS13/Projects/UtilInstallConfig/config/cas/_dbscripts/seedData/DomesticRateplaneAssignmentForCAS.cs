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
    public class DomesticRateplaneAssignmentForCAS : IScript
    {
        public override string ToString() => this.RuleName;
        public string RuleName => GetType().Name;
        public string HelpText => "";
        public ScriptType ScriptType => ScriptType.SqlSeedData;
        public string ScriptDir { get; set; }
        public string SrcTextFileName { get; set; } = "icxSp.sql";
        public string GetScript(object data)
        {
            Dictionary<string, object> input = (Dictionary<string, object>) data;
            MySqlConnection con = (MySqlConnection) input["con"];
            MySqlSession session= new MySqlSession(con);
            int maxIdOfRatePlanAssignTuple =
                session.ExecCommandAndGetScalerValue<int>("select max(id) as maxId from rateplanassignmenttuple");
            
            string sql = @"select idPartner from partner
                            where idPartner not in (select CountryCode from rateassign where inactive=32)
                            and PartnerType=2;";
            List<int> idPartnersWithNoDomesticRatePlan = session.ExecCommandAndGetSingleColList<int>(sql);
            List<string> tupleInserts= new List<string>();
            List<string> billingRuleInserts= new List<string>();
            List<string> rateAssignInserts = new List<string>();

            int tupleId = maxIdOfRatePlanAssignTuple;
            foreach (var idPartner in idPartnersWithNoDomesticRatePlan)
            {
                tupleId++;
                string tupleInsert=
                    "INSERT INTO `btrc_cas`.`rateplanassignmenttuple` (`id`, `idService`, `AssignDirection`, `idpartner`, `priority`) " +
                    $"VALUES ('{tupleId}', '1', '1', '{idPartner}', '1')";

                string billingRuleInsert =
                    $@"INSERT INTO `btrc_cas`.`billingruleassignment` 
                    (`idRatePlanAssignmentTuple`, `idBillingRule`, `idServiceGroup`) VALUES ('{tupleId}', '2', '1')";
                billingRuleInserts.Add(billingRuleInsert);

                string sqlInsert =
                    $@"('{tupleId}', '100.00000000', '0', '0', '1', '1', '1', '1.00000000', '1', '{idPartner}', '2', '0', '0', '2000-01-02 00:00:00', '32', '-1', '-1', '-1', '0', '0', '0', '0.00000000', '0.00000000', '0', '0', '0', '0', '-1', '21600.00000000', '0', '0', '45', '2023-09-24 19:59:28', '2', '0', '0', '1', '1', '0', '1')";
                tupleInserts.Add(tupleInsert);
                rateAssignInserts.Add(sqlInsert);
            }
            
            string headerForRateAssign =
                "INSERT INTO `btrc_cas`.`rateassign` (`Prefix`, `rateamount`, `WeekDayStart`, `WeekDayEnd`, `Resolution`, `MinDurationSec`, `SurchargeTime`, `SurchargeAmount`, `idrateplan`, `CountryCode`, `field1`, `field2`, `field3`, `startdate`, `Inactive`, `RouteDisabled`, `Type`, `Currency`, `OtherAmount1`, `OtherAmount2`, `OtherAmount3`, `OtherAmount4`, `OtherAmount5`, `OtherAmount6`, `OtherAmount7`, `OtherAmount8`, `OtherAmount9`, `OtherAmount10`, `TimeZoneOffsetSec`, `RatePosition`, `IgwPercentageIn`, `ChangedByTaskId`, `ChangedOn`, `Status`, `idPreviousRate`, `EndPreviousRate`, `Category`, `SubCategory`, `ChangeCommitted`, `BillingParams`) VALUES ";
            string script =
                string.Join(";\r\n", tupleInserts) + ";" +
                headerForRateAssign + "\r\n" +
                string.Join(",\r\n", rateAssignInserts) + ";" + "\r\n" +
                string.Join(";\r\n", billingRuleInserts);

            return script;
        }
    }
}
