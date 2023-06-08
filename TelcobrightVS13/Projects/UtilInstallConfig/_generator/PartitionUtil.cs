using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibraryExtensions.ConfigHelper;
using MySql.Data.MySqlClient;

namespace InstallConfig
{
    public static class PartitionUtil
    {
        public static void ModifyPartitions(DatabaseSetting databaseSetting, string operatorName)
        {
            string operatorDatabaseName = databaseSetting.DatabaseName;
            List<string> partitionedTables = databaseSetting.PartitionedTables;
            int MaxPartitionsPerTable = databaseSetting.MaxPartitionsPerTable;
            DateTime partitionStartDate = databaseSetting.PartitionStartDate;
            Console.WriteLine($@"All partitions before {partitionStartDate.ToString("yyyy-MM-dd")} will be erased. Are you sure? (Y/N)");
            var consoleKey = Console.ReadKey();
            Console.WriteLine();
            if (consoleKey.KeyChar != 'y' && consoleKey.KeyChar != 'Y')
            {
                Console.WriteLine("No partitions were changed.");
                return;
            }
            Console.WriteLine("Opeing multiple connections for modifying partitions...");
            string storageEngineForPartitionedTables = databaseSetting.StorageEngineForPartitionedTables;
            int partitionLenInDays = databaseSetting.PartitionLenInDays;
            string constr =
                "server=" + databaseSetting.ServerName + ";User Id=" + databaseSetting.AdminUserName +
                ";password=" + databaseSetting.AdminPassword + ";Persist Security Info=True; default command timeout=3600";

            Dictionary<string, TablePartitionManager> tableWisePartitionManager = new Dictionary<string, TablePartitionManager>();
            partitionedTables.ForEach(t =>
            {
                TablePartitionManager partitionManager = new TablePartitionManager(constr);
                tableWisePartitionManager.Add(t, partitionManager);
            });

            using (MySqlConnection con = new MySqlConnection(constr))
            {
                con.Open();
                Dictionary<string, List<PartitionInfo>> allPartitions = getAllPartitionsInfo(con, databaseSetting, operatorDatabaseName)
                    .GroupBy(p => p.TableName).ToDictionary(grouped => grouped.Key,
                        grouped => grouped.OrderBy(item => item.PartitionOrdinalPosition).ToList());
                foreach (string table in partitionedTables)
                {
                    List<PartitionInfo> partitionInfos = allPartitions[table];
                    string partitionExpression = partitionInfos.First().PartitionExpression;
                    Console.WriteLine($@"####Modifying partitions for table: {table}, start date={partitionStartDate.ToString("yyyy-MM-dd")}");
                    Console.WriteLine($@"####Partition Length={partitionLenInDays} days");

                    List<string> oldPartitionsToDelete = partitionInfos.Where(p => p.PartitionDate < partitionStartDate)
                        .OrderBy(p => p.PartitionOrdinalPosition).Select(p => p.PartitionName).ToList();
                    List<PartitionInfo> newPartitionsToCreate = new List<PartitionInfo>();

                    int lastPartitionNumber = partitionInfos.Last().PartitionNumber;
                    DateTime lastPartitionDate = partitionInfos.Last().PartitionDate;
                    DateTime firstPartitionDate = partitionInfos.First().PartitionDate;
                    if (partitionStartDate <= firstPartitionDate)
                    {
                        Console.WriteLine($@"Partition startdate must be > last existing max partition {lastPartitionDate.ToString("yyyy-MM-dd")}");
                        Console.WriteLine($@"No Partitions are changed. Press any key to exit.");
                        Console.ReadKey();
                        Console.WriteLine();
                        return;
                    }
                    int remainingPartitionsAfterDelete = partitionInfos.Count - oldPartitionsToDelete.Count;//5

                    int maxNewPartitions = MaxPartitionsPerTable - oldPartitionsToDelete.Count;
                    int nextPartitionNumber = lastPartitionNumber + 1;
                    int maxPartitionNumberNew = lastPartitionNumber + maxNewPartitions;
                    DateTime nextPartitionDate = partitionStartDate > lastPartitionDate ? partitionStartDate
                        : lastPartitionDate.AddDays(partitionLenInDays);

                    while (nextPartitionNumber <= maxPartitionNumberNew)
                    {
                        PartitionInfo newPartition = new PartitionInfo()
                        {
                            TableName = table,
                            PartitionName = "p" + nextPartitionNumber,
                            PartitionDate = nextPartitionDate,
                            PartitionDescription = "'" + nextPartitionDate.ToString("yyyy-MM-dd") + "'",
                            PartitionExpression = partitionExpression
                        };
                        newPartitionsToCreate.Add(newPartition);
                        nextPartitionNumber++;
                        nextPartitionDate = nextPartitionDate.AddDays(partitionLenInDays);
                    }
                    TablePartitionManager partitionManager = tableWisePartitionManager[table];
                    partitionManager.PartitionCreateinfo = newPartitionsToCreate;
                    partitionManager.PartitionDropinfo = oldPartitionsToDelete;
                    partitionManager.tableName = table;

                }
                //write sqls to drop and create partitions
                tableWisePartitionManager.Values.AsParallel().ForAll(pManager =>
                {
                    Func<PartitionInfo, string> getCreateScript = (p) =>
                    {
                        return $@"PARTITION {p.PartitionName} VALUES LESS THAN ({p.PartitionDescription}) ENGINE={storageEngineForPartitionedTables}";
                    };
                    MySqlConnection con2 = pManager.con;
                    var oldPartitionsToDelete = pManager.PartitionDropinfo;
                    var newPartitionsToCreate = pManager.PartitionCreateinfo;
                    using (MySqlCommand cmd = new MySqlCommand("", con2))
                    {
                        cmd.CommandText = $@"use {operatorDatabaseName};";
                        cmd.ExecuteNonQuery();

                        oldPartitionsToDelete.Reverse();
                        var partitionsToDeleteExceptOne = oldPartitionsToDelete.Skip(1).ToList();//mysql doesn't support 0 partition
                        partitionsToDeleteExceptOne.ForEach(pName =>
                        {
                            cmd.CommandText = $@"alter table {pManager.tableName} drop Partition {pName}";
                            Console.WriteLine($@"{pManager.tableName} drop: partition {pName}");
                            cmd.ExecuteNonQuery();
                        });

                        newPartitionsToCreate.ForEach(p =>
                        {
                            var script = getCreateScript(p);
                            cmd.CommandText = $@"alter table {pManager.tableName} add partition({script});";
                            Console.WriteLine($@"{pManager.tableName} add: {script}");
                            cmd.ExecuteNonQuery();
                        });
                    }
                    con.Close();
                    con.Dispose();
                });
                Console.WriteLine("Partitions were modified successfully!!!");
            }
        }
        private static List<PartitionInfo> getAllPartitionsInfo(MySqlConnection con, DatabaseSetting databaseSetting,
            string operatorDbName)
        {
            using (MySqlCommand cmd = new MySqlCommand("", con))
            {
                cmd.CommandText = $@"select 
                                        p.Table_Name as TableName, 
                                        Partition_Name as PartitionName,
                                        Partition_ordinal_position as PartitionOrdinalPosition,
                                        partition_expression as PartitionExpression,
                                        partition_description as PartitionDescription
                                        from information_schema.partitions p
                                        left join 
                                        (select * from information_schema.tables
                                        where TABLE_SCHEMA='{operatorDbName}'
                                        and table_name not like 'lcr%'
                                        and table_name not like 'rate%') t
                                        on p.table_name=t.table_name
                                        where p.TABLE_SCHEMA='{operatorDbName}'
                                        and p.partition_name is not null
                                        and p.table_name not like 'lcr%'
                                        and p.table_name not like 'rate%'
                                        order by PartitionOrdinalPosition";
                MySqlDataReader reader = cmd.ExecuteReader();
                List<PartitionInfo> partionInfos = new List<PartitionInfo>();
                while (reader.Read())
                {
                    PartitionInfo partitionInfo = new PartitionInfo()
                    {
                        TableName = reader[0].ToString(),
                        PartitionName = reader[1].ToString(),
                        PartitionOrdinalPosition = Convert.ToInt32(reader[2].ToString()),
                        PartitionExpression = reader[3].ToString(),
                        PartitionDescription = reader[4].ToString().Replace("'", "")
                    };
                    DateTime tempDate = new DateTime();
                    if (!DateTime.TryParseExact(partitionInfo.PartitionDescription, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate))
                    {
                        throw new Exception("Could not parse partion expression to valid datetime.");
                    }
                    partitionInfo.PartitionDate = tempDate;
                    partitionInfo.PartitionNumber = Convert.ToInt32(partitionInfo.PartitionName.Substring(1));
                    partionInfos.Add(partitionInfo);
                }
                reader.Close();
                return partionInfos;
            };
        }
    }
}
