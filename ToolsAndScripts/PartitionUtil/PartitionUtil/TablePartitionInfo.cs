using System;
using System.Collections.Generic;
using System.Linq;

namespace PartitionUtil
{
    public class TablePartitionInfo
    {
        public static int MAX_PARTITION_COUNT = 1025;
        public List<SinglePartition> Partitions { get; }
        public string TableName { get; }
        public int MinPartitionNo { get; }
        public int MaxPartitionNo { get; }
        public int PartitionCount { get; }
        public DateTime MinPartitionDate { get; }
        public DateTime MaxPartitionDate { get; }
        public string PartitionExpression { get; }
        public string Engine { get; }
        public TablePartitionInfo(string tableName, List<SinglePartition> partitions)
        {
            if(!partitions.Any()) throw new Exception("No partition information for table.");
            this.Partitions = partitions;
            var firstPartition = this.Partitions.First();
            this.PartitionExpression = firstPartition.PartitionExpression;
            this.TableName = tableName;
            this.MinPartitionNo = this.Partitions.Min(p => p.PartitionNumber);
            this.MaxPartitionNo = this.Partitions.Max(p => p.PartitionNumber);
            this.PartitionCount = this.MaxPartitionNo - this.MinPartitionNo + 1;
            this.MinPartitionDate = this.Partitions.Min(p => p.PartitionDate);
            this.MaxPartitionDate = this.Partitions.Max(p => p.PartitionDate);
            this.Engine = firstPartition.Engine;
        }

        
    }
}