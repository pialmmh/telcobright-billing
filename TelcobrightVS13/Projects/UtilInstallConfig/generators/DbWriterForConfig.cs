using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions;
using LibraryExtensions.ConfigHelper;
using MediationModel;
using MySql.Data.MySqlClient;
using TelcobrightInfra;
using TelcobrightMediation;
using TelcobrightMediation.Config;

namespace InstallConfig._generator
{
    public enum SqlOperationType
    {
        DML,
        DDL,
        SeedData
    }

    public class DbWriterForConfig
    {
        public TelcobrightConfig Tbc { get; }
        public ConfigPathHelper ConfigPathHelper { get; }
        private MySqlConnection Con { get; }
        private MySqlCommand Cmd { get; }
        public Dictionary<string, IScript> DdlScripts { get; }

        public DbWriterForConfig(TelcobrightConfig tbc, ConfigPathHelper configPathHelper,
            MySqlConnection con, Dictionary<string, IScript> ddlScripts)
        {
            this.Tbc = tbc;
            this.Con = con;
            if (this.Con.State != ConnectionState.Open) this.Con.Open();
            this.ConfigPathHelper = configPathHelper;
            this.Cmd = new MySqlCommand("", this.Con);
            this.DdlScripts = ddlScripts;
        }

        public void WriteTelcobrightPartnerAndNes()
        {
            Console.WriteLine("Loading partner and nes for " + this.Tbc.Telcobrightpartner.databasename + "...");
            List<telcobrightpartner> partners = new List<telcobrightpartner>()
            {
                this.Tbc.Telcobrightpartner
            };
            partners.Insert(0, DummyTelcobrightPartner.getDummyTelcobrightPartner());

            List<ne> nes = this.Tbc.Nes;
            nes.Insert(0, DummySwitch.getDummyNe());
            executeScript("SET FOREIGN_KEY_CHECKS = 0;");

            executeScript("ALTER TABLE ne DISABLE KEYS;");
            executeScript("ALTER TABLE telcobrightpartner DISABLE KEYS;");

            executeScript("truncate table ne;");
            executeScript("truncate table telcobrightpartner;");

            executeScript("insert into telcobrightpartner values " +
                          string.Join(",", partners.Select(p => p.GetExtInsertValues())));
            executeScript("update telcobrightpartner set idCustomer=0 where CustomerName = 'Dummy'");




            executeScript("insert into ne values " +
                          string.Join(",", nes.Select(p => p.GetExtInsertValues())));
            executeScript("update ne set idSwitch=0 where SwitchName = 'dummy'");
            executeScript("SET FOREIGN_KEY_CHECKS = 1;");
            executeScript("ALTER TABLE ne enable KEYS;");
            executeScript("ALTER TABLE telcobrightpartner enable KEYS;");




            Console.WriteLine("Finished Loading partner and nes for " + this.Tbc.Telcobrightpartner.databasename + ".");
        }

        public void executeScript(string sql)
        {
            MySqlScript script = new MySqlScript(this.Con, sql);
            script.Execute();
        }

        public void LoadSeedDataSqlForTelcoBilling(SqlOperationType sqlOperationType)
        {
            string msgBeforeOp;
            string msgAfterOp;
            string sqlDir;

            switch (sqlOperationType)
            {

                case SqlOperationType.DML:
                    sqlDir = this.ConfigPathHelper.getTelcoBillingDmlSqlHome();
                    msgBeforeOp = "Loading dml scrips for " + this.Tbc.Telcobrightpartner.databasename + "...";
                    msgAfterOp = "Finished loading dml scrips for " + this.Tbc.Telcobrightpartner.databasename + ".";
                    break;
                case SqlOperationType.DDL:
                    sqlDir = this.ConfigPathHelper.getTelcoBillingDdlSqlHome();
                    msgBeforeOp = "Loading ddl scrips for " + this.Tbc.Telcobrightpartner.databasename + "...";
                    msgAfterOp = "Finished loading ddl scrips for " + this.Tbc.Telcobrightpartner.databasename + ".";
                    break;
                case SqlOperationType.SeedData:
                    sqlDir = this.ConfigPathHelper.getTelcoBillingSeedDataSqlHome();
                    msgBeforeOp = "Loading seed data for " + this.Tbc.Telcobrightpartner.databasename + "...";
                    msgAfterOp = "Finished loading seed data for " + this.Tbc.Telcobrightpartner.databasename + ".";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sqlOperationType), sqlOperationType, null);
            }


            if (sqlOperationType != SqlOperationType.DDL)
            {
                Console.WriteLine(msgBeforeOp);
                DirectoryInfo d = new DirectoryInfo(sqlDir);
                FileInfo[] sqlFiles = d.GetFiles("*.sql");
                foreach (FileInfo file in sqlFiles)
                {
                    List<string> lines = File.ReadAllLines(file.FullName).ToList();
                    if (lines[0].ToLower().StartsWith("#skip")) continue;
                    string sql = string.Join("", lines); //
                    Console.WriteLine("Loading " + file.Name);
                    if (this.Con.State != ConnectionState.Open)
                    {
                        this.Con.Open();
                    }
                    sql = $@"SET FOREIGN_KEY_CHECKS = 0;
                      {sql} 
                      SET FOREIGN_KEY_CHECKS = 1;";
                    executeScript(sql);
                }
                Console.WriteLine(msgAfterOp);
            }
        }
    }
}
