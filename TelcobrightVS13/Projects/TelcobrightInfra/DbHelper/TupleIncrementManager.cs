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

        public static void getIncrementalValue(string tuple, MySqlConnection con)
        {
            //string connectionString = "server=localhost;user=root;password=;database=testdb;";
            //MySqlConnection con = new MySqlConnection(connectionString);
            //con.Open();

            // search using given tuple
            string searchSQL = $@"  select tuple, lastValue 
                                    from testTable 
                                    where tuple = '{tuple}'";
            MySqlCommand searchCmd = new MySqlCommand(searchSQL, con);
            MySqlDataReader reader = searchCmd.ExecuteReader();
            

            // if tuple already exist then increase lastValue
            if (reader.Read())
            {

                int lastValue = reader.GetInt32("lastValue");
                string updateSQL = $@"  update testTable 
                                        set lastValue = {lastValue + 1}
                                        where tuple = '{tuple}'; ";
                MySqlCommand updateCmd = new MySqlCommand(updateSQL, con);
                reader.Close();
                updateCmd.ExecuteNonQuery();
            }
            // if tuple does not exist then insert new tuple with lastValue = 1
            else
            {
                string insertSQL = $@"  insert into testTable 
                                        values ( '{tuple}', {1}); ";
                MySqlCommand updateCmd = new MySqlCommand(insertSQL, con);
                reader.Close();
                updateCmd.ExecuteNonQuery();
            }
        }
    }
}
