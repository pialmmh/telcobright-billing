using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace LibraryExtensions.ConfigHelper
{
    public class PartitionManagerPerTable
    {
        Dictionary<string, DateTime> ExistingPartitionInfo { get; set; }
        private string TableName { get;}
        private string PartitionColumnName { get;}
        public DateTime PartitionStartDate { get; }
        public int NoOfPartitions { get; }
        public string DatabaseEngine { get; }
        MySqlConnection Con { get; set; }
        private MySqlCommand Cmd { get; set; }
        
        public PartitionManagerPerTable(string tableNameWithPartionColName,
            DateTime partitionStartDate,int noOfPartitions,string databaseEngine,
            MySqlConnection con)
        {
            this.PartitionStartDate = partitionStartDate;
            this.NoOfPartitions = noOfPartitions;
            this.DatabaseEngine = databaseEngine;
            var arr = tableNameWithPartionColName.Split('/');
            this.TableName = arr[0];
            this.PartitionColumnName = arr[1];
            this.Con=con;
            if(con.State!=ConnectionState.Open) this.Con.Open();
            this.Cmd=new MySqlCommand("",this.Con);
        }

        public void PopulateExistingPartitionInfo()
        {

        }
        public void ResetPartitions()
        {
            string createTableInfo = GetQueryResultsByDataReader("show create table " + this.TableName).First()
                .Replace("*","").Replace("/","");
            string sqlCreateTableWithSingleDummyPartition =
                createTableInfo.Split(new string[] { "ENGINE" }, StringSplitOptions.None)[0]
                + "ENGINE="+this.DatabaseEngine+" DEFAULT CHARSET=utf8 COLLATE=utf8_bin "
                + "PARTITION BY RANGE COLUMNS("+this.PartitionColumnName+") "
                + "(PARTITION P0 VALUES LESS THAN ('1800-01-01') ENGINE = "+this.DatabaseEngine+");";
            CreateTableWithSingleDummyPartition(sqlCreateTableWithSingleDummyPartition);
            CreatePartitions();
            Console.WriteLine("Finished creating partitions for table " + this.TableName + ".");
        }
        
        void CreateTableWithSingleDummyPartition(string createTableInfoWithoutPartition)
        {
            Console.WriteLine("Dropping table "+this.TableName+"...");
            this.Cmd.CommandText = $@"drop table if exists {this.TableName}";
            this.Cmd.ExecuteNonQuery();
            Console.WriteLine("Creating table "+this.TableName+" without partitions...");
            this.Cmd.CommandText = createTableInfoWithoutPartition;
            this.Cmd.ExecuteNonQuery();
        }

        void CreatePartitions()
        {
            int startPartitionNo = 1;
            int endPartitionNo = startPartitionNo + 1019; //use total 1020, keep 4 spares
            DateTime startDate = this.PartitionStartDate;
            DateTime endDate = this.PartitionStartDate.AddDays(this.NoOfPartitions - 1);
            List<KeyValuePair<int, DateTime>> partitionNoWithDates = Enumerable
                .Range(0, 1 + endDate.Subtract(startDate).Days)
                .Select(offset => new KeyValuePair<int, DateTime>(offset + 1, startDate.AddDays(offset))).ToList();
            List<string> alterTableAddPartitionSqls =
                partitionNoWithDates.Select(c => "alter table "+this.TableName+" add PARTITION ("+
                            "PARTITION P"+Convert.ToInt32(c.Key)+" VALUES LESS THAN ('"+c.Value.Date.ToString("yyyy-MM-dd")+"') ENGINE = "+this.DatabaseEngine+
                            ");").ToList();
            Console.WriteLine("Creating partitions for table "+this.TableName+"...");
            foreach (string alterTableAddPartitionSql in alterTableAddPartitionSqls)
            {
                this.Cmd.CommandText = alterTableAddPartitionSql;
                Console.WriteLine(alterTableAddPartitionSql);
                this.Cmd.ExecuteNonQuery();
            }
        }

        List<string> GetQueryResultsByDataReader(string query)
        {
            List<string> queryResults = new List<string>();
            this.Cmd.CommandText = query;
            using (MySqlDataReader reader = this.Cmd.ExecuteReader())
            {
                queryResults = reader.ConvertToList<string>(dataItem => dataItem[1].ToString());
            }
            return queryResults;
        }
    }
}
