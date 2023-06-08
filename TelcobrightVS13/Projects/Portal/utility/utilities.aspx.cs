using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MediationModel;

namespace PortalApp.utility
{
    public partial class Utilities : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Update_Click(object sender, EventArgs e)
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                List<string> names = context.Database.SqlQuery<string>(
               @"select table_name from information_schema.tables
where table_schema='telcobrightmediation'
and table_name like 'enum%'").ToList();
                using (MySqlConnection con = new MySqlConnection(ConfigurationManager.
                    ConnectionStrings["Partner"].ConnectionString))
                {
                    List<string> lstsql = new List<string>();
                    foreach (string tname in names)
                    {
                        string newsql = " create or replace view "+ tname +" as select * from " + tname + " ";
                        lstsql.Add(newsql);
                    }
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand("", con))
                    {
                        cmd.CommandText = string.Join(";", lstsql) +";";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        protected void Buttonacc_Click(object sender, EventArgs e)
        {
           
        }

        //account CreateAccount(string Accname,int type, int Partnerid,int Currency)
        //{
        //    account na = new account();
        //    na.AccountName = Accname;
        //    na.AccountType = type;
        //    na.idpartner = Partnerid;
        //    na.Currency = Currency;
        //    return na;
        //}
    }
}