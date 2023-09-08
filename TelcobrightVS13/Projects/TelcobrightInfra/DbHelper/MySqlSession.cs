using System;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
namespace TelcobrightInfra
{
    public class MySqlSession
    {
        public MySqlConnection Con { get; }
        public MySqlCommand Cmd { get; }

        public MySqlSession(MySqlConnection con)
        {
            this.Con = con;
            this.Cmd = new MySqlCommand("",this.Con);
            if (con.State != ConnectionState.Open) con.Open();
        }

        public void executeCommand(string sql)
        {
            this.Cmd.CommandText = sql;
            this.Cmd.ExecuteNonQuery();
        }
    }
}