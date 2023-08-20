using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace TelcobrightInfra
{
    public class TupleIncrementManager
    {
        //static void Main(string[] args)
        //{
        //    string tuple = "Mustafa";
        //    GetIncrementalValue(tuple);
        //}

        public int DefaultValue = 1000;
        public TupleIncrementManager(int defaultValue)
        {
            this.DefaultValue = defaultValue;
        }

        public void getIncrementalValue(string tuple, MySqlConnection con)
        {
            //string connectionString = "server=localhost;user=root;password=;database=testdb;";
            //MySqlConnection con = new MySqlConnection(connectionString);
            //con.Open();

            // search using given tuple
            string searchSql = $@"  select tuple, lastValue 
                                    from testTable 
                                    where tuple = '{tuple}'";
            MySqlCommand searchCmd = new MySqlCommand(searchSql, con);
            MySqlDataReader reader = searchCmd.ExecuteReader();
            

            // if tuple already exist then increase lastValue
            if (reader.Read())
            {

                int lastValue = reader.GetInt32("lastValue");
                string updateSql = $@"  update testTable 
                                        set lastValue = {lastValue + 1}
                                        where tuple = '{tuple}'; ";
                MySqlCommand updateCmd = new MySqlCommand(updateSql, con);
                reader.Close();
                updateCmd.ExecuteNonQuery();
            }
            // if tuple does not exist then insert new tuple with lastValue = 1
            else 
            {
                string insertSql = $@"  insert into testTable 
                                        values ( '{tuple}', {this.DefaultValue}); ";
                MySqlCommand updateCmd = new MySqlCommand(insertSql, con);
                reader.Close();
                updateCmd.ExecuteNonQuery();
            }
        }
    }
}
