using System;

namespace PartitionUtil
{
    public class SinglePartition
    {
        public string TableName { get; set; }
        public string Engine { get; set; }
        public string PartitionName { get; set; }
        public int PartitionNumber { get; set; }
        public string PartitionExpression { get; set; }
        public DateTime PartitionDate { get; set; }
    }
}