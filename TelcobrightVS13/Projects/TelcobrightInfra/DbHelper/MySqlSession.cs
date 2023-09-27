using System;
using System.Collections.Generic;
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
        public List<T> ExecCommandAndGetSingleColList<T>(string commandText)
        {
            MySqlCommand cmd = new MySqlCommand("", this.Con);
            cmd.CommandText = commandText;
            MySqlDataReader reader = cmd.ExecuteReader();
            List<T> retVals= new List<T>();
            while (reader.Read())
            {
                retVals.Add((T)reader[0]);
            }
            reader.Close();
            return retVals;
        }

        public string ExecCommandAndGetScalerString(string commandText)
        {
            MySqlCommand cmd = new MySqlCommand("", this.Con);
            cmd.CommandText = commandText;
            MySqlDataReader reader = cmd.ExecuteReader();
            StringBuilder sb = new StringBuilder();
            while (reader.Read())
            {
                sb.Append(reader[0].ToString());
            }
            reader.Close();
            return sb.ToString();
        }

        public T ExecCommandAndGetScalerValue<T>(string commandText)
        {
            MySqlCommand cmd = new MySqlCommand("", this.Con);
            cmd.CommandText = commandText;
            MySqlDataReader reader = cmd.ExecuteReader();
            T retVal= default(T);
            while (reader.Read())
            {
                retVal= (T)reader[0];
            }
            reader.Close();
            return retVal;
        }


    }
}