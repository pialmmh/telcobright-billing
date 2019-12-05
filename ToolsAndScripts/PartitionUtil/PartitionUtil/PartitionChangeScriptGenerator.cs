using System;
using System.Linq;
using System.Text;

namespace PartitionUtil
{
    public class PartitionChangeScriptGenerator
    {
        private TablePartitionInfo tablePartitionInfo;
        private DateTime deleteBeforePartitionDate;
        private int noOfNewPartitionsToAdd;
        public PartitionChangeScriptGenerator(TablePartitionInfo tablePartitionInfo, DateTime deleteBeforePartitionDate,
            int noOfNewPartitionsToAdd)
        {
            this.tablePartitionInfo = tablePartitionInfo;
            this.deleteBeforePartitionDate = deleteBeforePartitionDate;
            if (deleteBeforePartitionDate < tablePartitionInfo.MinPartitionDate ||
                deleteBeforePartitionDate > tablePartitionInfo.MaxPartitionDate)
                throw new Exception(
                    $"Delete before partition date must be between {tablePartitionInfo.MinPartitionDate.Date} " +
                    $" and {tablePartitionInfo.MaxPartitionDate.Date}");
            this.noOfNewPartitionsToAdd = noOfNewPartitionsToAdd <= TablePartitionInfo.MAX_PARTITION_COUNT
                ? noOfNewPartitionsToAdd
                : TablePartitionInfo.MAX_PARTITION_COUNT;
        }

        public StringBuilder GenerateScript()
        {
            var partitionsToDel =
                tablePartitionInfo.Partitions.Where(p => p.PartitionDate < this.deleteBeforePartitionDate)
                    .Select(p => p.PartitionName).ToList();

            StringBuilder script = 
                new StringBuilder(!partitionsToDel.Any()?"#":string.Empty)
                .Append($"alter table {tablePartitionInfo.TableName} drop partition {string.Join(",", partitionsToDel)};")
                .Append(Environment.NewLine);

            var newPartitions = Enumerable.Range(tablePartitionInfo.MaxPartitionNo + 1, this.noOfNewPartitionsToAdd)
                .Select(num =>
                {
                    int increment = num - this.tablePartitionInfo.MaxPartitionNo;
                    return new SinglePartition()
                    {
                        PartitionNumber = this.tablePartitionInfo.MaxPartitionNo+increment,
                        PartitionDate = this.tablePartitionInfo.MaxPartitionDate.AddDays(increment),
                        PartitionExpression = this.tablePartitionInfo.PartitionExpression
                    };
                }).ToList();

            Func<SinglePartition, string> addPartitionClause = p =>
                $"PARTITION p{p.PartitionNumber} VALUES LESS THAN ('{p.PartitionDate:yyyy-MM-dd}') ENGINE={tablePartitionInfo.Engine}";
            script.Append($"alter table {this.tablePartitionInfo.TableName} add partition({Environment.NewLine}" +
                                  string.Join($",{Environment.NewLine}",
                                      newPartitions.Select(p => addPartitionClause(p))) + $");")
                                      .Append(Environment.NewLine);
            return script;

        }
    }
}