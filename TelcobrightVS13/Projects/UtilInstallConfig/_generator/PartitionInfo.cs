using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstallConfig
{
    public class PartitionInfo
    {
        public string TableName { get; set; }
        public string PartitionName { get; set; }
        public int PartitionNumber { get; set; }
        public int PartitionOrdinalPosition { get; set; }
        public string PartitionExpression { get; set; }
        public string PartitionDescription { get; set; }
        public DateTime PartitionDate { get; set; }
    }
}
