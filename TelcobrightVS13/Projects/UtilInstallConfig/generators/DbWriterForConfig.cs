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
        private PartnerEntities Context { get; }
        private MySqlConnection Con { get; }
        private MySqlCommand Cmd { get; set; }

        public DbWriterForConfig(TelcobrightConfig tbc, ConfigPathHelper configPathHelper,
            PartnerEntities context, MySqlConnection con)
        {
            this.Tbc = tbc;
            this.Context = context;
            this.Con = con;
            if (this.Con.State != ConnectionState.Open) this.Con.Open();
            this.ConfigPathHelper = configPathHelper;
            this.Cmd = new MySqlCommand("", this.Con);
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
            executeQuery("SET FOREIGN_KEY_CHECKS = 0;");

            executeQuery("ALTER TABLE ne DISABLE KEYS;");
            executeQuery("ALTER TABLE telcobrightpartner DISABLE KEYS;");

            executeQuery("delete from ne;");
            executeQuery("delete from telcobrightpartner;");

            this.Context.telcobrightpartners.AddRange(partners);

            executeQuery("insert into telcobrightpartner values " +
                                                    string.Join(",", partners.Select(p => p.GetExtInsertValues())));
            executeQuery("update telcobrightpartner set idCustomer=0 where CustomerName = 'Dummy'");
            executeQuery("insert into ne values " +
                         string.Join(",", nes.Select(p => p.GetExtInsertValues())));
            executeQuery("update ne set idSwitch=0 where SwitchName = 'dummy'");

            executeQuery("SET FOREIGN_KEY_CHECKS = 1;");
            executeQuery("ALTER TABLE ne DISABLE KEYS;");
            executeQuery("ALTER TABLE telcobrightpartner DISABLE KEYS;");


            Console.WriteLine("Finished Loading partner and nes for " + this.Tbc.Telcobrightpartner.databasename + ".");
        }

        private void executeQuery(string sql)
        {
            this.Cmd.CommandText = sql;
            this.Cmd.ExecuteNonQuery();
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


            Console.WriteLine(msgBeforeOp);
            DirectoryInfo d = new DirectoryInfo(sqlDir);
            FileInfo[] sqlFiles = d.GetFiles("*.sql");
            foreach (FileInfo file in sqlFiles)
            {
                Console.WriteLine("Loading " + file.Name);
                string sql = File.ReadAllText(file.FullName);
                using (MySqlCommand cmd = new MySqlCommand("", this.Con))
                {
                    if (this.Con.State != ConnectionState.Open)
                    {
                        this.Con.Open();
                    }
                    executeQuery("SET FOREIGN_KEY_CHECKS = 0;");

                    executeQuery(sql);

                    executeQuery("SET FOREIGN_KEY_CHECKS = 1;");
                }
            }
            Console.WriteLine(msgAfterOp);
        }
    }
}
