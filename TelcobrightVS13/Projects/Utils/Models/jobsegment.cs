namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.jobsegment")]
    public partial class jobsegment
    {
        public long id { get; set; }

        public long idJob { get; set; }

        public int segmentNumber { get; set; }

        public int stepsCount { get; set; }

        public int? status { get; set; }

        [StringLength(1073741823)]
        public string SegmentDetail { get; set; }

        public virtual job job { get; set; }
    }
}
