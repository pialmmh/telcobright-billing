using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Process
{
    public class PartitionInfo
    {
        public string TableName { get; set; }
        public string PartitionName { get; set; }
        public int PartitionOrdinalPosition { get; set; }
        public DateTime PartitionDate { get; set; }
    }

    public class PartitionUtil
    {
        private readonly MySqlConnection _connection;
        private readonly string _databaseName;
        private readonly int _daysToDeleteAndAdd;

        public Dictionary<string, List<string>> PartitionSqls { get; } = new Dictionary<string, List<string>>();

        public PartitionUtil(MySqlConnection connection, string databaseName, int daysToDeleteAndAdd)
        {
            _connection = connection;
            _databaseName = databaseName;
            _daysToDeleteAndAdd = daysToDeleteAndAdd;
        }

        public List<PartitionInfo> GetPartitionInfo()
        {
            var partitions = new List<PartitionInfo>();
            string query = $@"
                SELECT p.TABLE_NAME, p.PARTITION_NAME, p.PARTITION_ORDINAL_POSITION, p.PARTITION_DESCRIPTION
                FROM information_schema.partitions p
                LEFT JOIN (SELECT * FROM information_schema.tables
                           WHERE TABLE_SCHEMA = '{_databaseName}'
                             AND TABLE_NAME NOT LIKE 'lcr%'
                             AND TABLE_NAME NOT LIKE 'rate%') t
                ON p.TABLE_NAME = t.TABLE_NAME
                WHERE p.TABLE_SCHEMA = '{_databaseName}'
                  AND p.PARTITION_NAME IS NOT NULL
                  AND p.TABLE_NAME NOT LIKE 'lcr%'
                  AND p.TABLE_NAME NOT LIKE 'rate%'";

            var cmd = new MySqlCommand(query, _connection);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var tableName = reader.GetString("TABLE_NAME");
                    var partitionName = reader.GetString("PARTITION_NAME");
                    var position = reader.GetInt32("PARTITION_ORDINAL_POSITION");
                    var desc = reader.GetString("PARTITION_DESCRIPTION").Replace("'", "");
                    DateTime tempDate = new DateTime();
                    DateTime partitionDate;
                    if (DateTime.TryParseExact(desc, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out tempDate))
                    {
                        partitionDate = tempDate;
                    }
                    else
                    {
                        throw new Exception("Couldnt' parse datetime from partition info");
                    }
                    partitions.Add(new PartitionInfo
                    {
                        TableName = tableName,
                        PartitionName = partitionName,
                        PartitionOrdinalPosition = position,
                        PartitionDate = partitionDate
                    });
                }
                reader.Close();
            }
            return partitions;
        }

        public void GeneratePartitionMaintenanceSql()
        {
            var partitions = GetPartitionInfo();
            var groupedByTable = new Dictionary<string, List<PartitionInfo>>();
            foreach (var p in partitions)
            {
                if (!groupedByTable.ContainsKey(p.TableName))
                    groupedByTable[p.TableName] = new List<PartitionInfo>();
                groupedByTable[p.TableName].Add(p);
            }

            foreach (var kvp in groupedByTable)
            {
                var tableName = kvp.Key;
                var partList = kvp.Value;
                partList.Sort((a, b) => a.PartitionDate.CompareTo(b.PartitionDate));
                var sqls = new List<string>();

                // Drop oldest _daysToDeleteAndAdd partitions
                for (int i = 0; i < Math.Min(_daysToDeleteAndAdd, partList.Count); i++)
                {
                    var p = partList[i];
                    sqls.Add($"ALTER TABLE `{tableName}` DROP PARTITION `{p.PartitionName}`;");
                }

                // Add _daysToDeleteAndAdd new partitions
                if (partList.Count > 0)
                {
                    var lastPartition = partList[partList.Count - 1];
                    var lastDate = lastPartition.PartitionDate;
                    var maxOrdinal = 0;
                    foreach (var p in partList)
                    {
                        var partNumStr = p.PartitionName.StartsWith("p") ? p.PartitionName.Substring(1) : p.PartitionName;
                        int partNum;
                        if (int.TryParse(partNumStr, out partNum) && partNum > maxOrdinal)
                        {
                            maxOrdinal = partNum;
                        }
                    }
                    for (int i = 1; i <= _daysToDeleteAndAdd; i++)
                    {
                        var newDate = lastDate.AddDays(i);
                        var newPartName = $"p{maxOrdinal + i}";
                        var sql = $"ALTER TABLE `{tableName}` ADD PARTITION (PARTITION {newPartName} VALUES LESS THAN ('{newDate:yyyy-MM-dd}'))";
                        sqls.Add(sql);
                    }
                }

                PartitionSqls[tableName] = sqls;
            }
        }

        public void ExecutePartitionSqls()
        {
            foreach (var kvp in PartitionSqls)
            {
                foreach (var sql in kvp.Value)
                {
                    Console.WriteLine("Partition Maintenance is being performed");
                    Console.WriteLine(sql);
                    var cmd = new MySqlCommand(sql, _connection);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

}
