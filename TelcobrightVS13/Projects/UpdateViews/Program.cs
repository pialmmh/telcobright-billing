
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
namespace UpdateViews
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (PartnerEntities context = new PartnerEntities())
                {
                    List<string> Names = context.Database.SqlQuery<string>(
                   @"select table_name from information_schema.tables
where table_schema='telcobrightmediation'
and table_name like 'enum%'").ToList();
                    using (MySqlConnection con = new MySqlConnection(ConfigurationManager.
                        ConnectionStrings["Partner"].ConnectionString))
                    {
                        List<string> lstsql = new List<string>();
                        foreach (string tname in Names)
                        {
                            string Newsql = " create or replace view " + tname + " as select * from " + tname + " ";
                            lstsql.Add(Newsql);
                        }
                        con.Open();
                        using (MySqlCommand cmd = new MySqlCommand("", con))
                        {
                            cmd.CommandText = string.Join(";", lstsql) + ";";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                Console.WriteLine("Views Updated Successfully!");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.Write("Error: " + e.Message + Environment.NewLine + e.InnerException);
                Console.ReadLine();
            }
        }
       }
}
