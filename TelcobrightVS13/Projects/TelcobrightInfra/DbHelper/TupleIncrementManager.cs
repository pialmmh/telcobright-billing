using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;


namespace TelcobrightInfra
{
    public class TupleIncrementManager
    {
        private DbCommand Cmd { get; set; }
        public int DefaultValue { get; set; } //int enough, higher than int in mysql is bigint which is too big and not required now
        public TupleIncrementManager(DbCommand cmd,int defaultValue=1001)
        {
            this.DefaultValue = defaultValue;
            this.Cmd = cmd;
        }
        public int getIncrementalValue(string tuple)
        {
            //string connectionString = "server=localhost;user=root;password=;database=testdb;";
            //MySqlConnection con = new MySqlConnection(connectionString);
            //con.Open();
            string tableName =  "tupleIncrement";
            string createTable = $@"CREATE TABLE IF NOT EXISTS {tableName}(
                                    tuple varchar(255) primary key,
                                    lastValue int not null
                                );";
            this.Cmd.CommandText = createTable;
            this.Cmd.ExecuteNonQuery();

            // search using given tuple
            string searchSql = $@"  select tuple, lastValue 
                                    from {tableName} 
                                    where tuple = '{tuple}'";
            DbCommandReader<int?> reader = new DbCommandReader<int?>(this.Cmd,searchSql);//new helper class by Telcobright
            int? lastValue = reader.getScaler();
            int newValue = -1;
            if (lastValue == null)
            {
                this.Cmd.CommandText= $" insert into tuple (tuple,lastvalue) values'{tuple}',{this.DefaultValue};";
                this.Cmd.ExecuteNonQuery();
            }
            else
            {
                newValue = Convert.ToInt32(lastValue) + 1;
                this.Cmd.CommandText= $@" update {tableName} 
                                        set lastValue = {newValue}
                                        where tuple = '{tuple}'; ";
                this.Cmd.ExecuteNonQuery();
            }
            return newValue;
        }
    }
}
