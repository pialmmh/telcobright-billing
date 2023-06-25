using System;
using System.Collections.Generic;
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
    public class DbWriterForConfig
    {
        public TelcobrightConfig Tbc { get; }
        public ConfigPathHelper ConfigPathHelper { get; }
        private PartnerEntities Context { get; }
        private MySqlConnection Con { get; }

        public DbWriterForConfig(TelcobrightConfig tbc, ConfigPathHelper configPathHelper,
            PartnerEntities context, MySqlConnection con)
        {
            Tbc = tbc;
            Context = context;
            Con = con;
            ConfigPathHelper = configPathHelper;
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

            this.Context.Database.ExecuteSqlCommand("delete from ne;");
            this.Context.Database.ExecuteSqlCommand("delete from telcobrightpartner;");

            this.Context.telcobrightpartners.AddRange(partners);

            this.Context.Database.ExecuteSqlCommand("insert into telcobrightpartner values " +
                                                    string.Join(",", partners.Select(p => p.GetExtInsertValues())));
            this.Context.Database.ExecuteSqlCommand(
                "update telcobrightpartner set idCustomer=0 where CustomerName = 'Dummy'");
            this.Context.Database.ExecuteSqlCommand("insert into ne values " +
                                                    string.Join(",", nes.Select(p => p.GetExtInsertValues())));
            this.Context.Database.ExecuteSqlCommand("update ne set idSwitch=0 where SwitchName = 'dummy'");
            Console.WriteLine("Finished Loading partner and nes for " + this.Tbc.Telcobrightpartner.databasename + ".");
        }

        public void LoadSeedDataSqlForTelcoBilling()
        {
            Console.WriteLine("Loading seed data for " + this.Tbc.Telcobrightpartner.databasename + "...");
            string dataDir = this.ConfigPathHelper.getTelcoBillingSeedDataSqlHome();
            DirectoryInfo d = new DirectoryInfo(dataDir);
            FileInfo[] sqlFiles = d.GetFiles("*.sql");
            foreach (FileInfo file in sqlFiles)
            {
                Console.WriteLine("Loading " + file.Name);
                string sql = File.ReadAllText(file.FullName);
                using (MySqlCommand cmd = new MySqlCommand("", this.Con))
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }
            Console.WriteLine("Finished loading seed data for " + this.Tbc.Telcobrightpartner.databasename + ".");
        }

    }
}
