namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.qrtz_simple_triggers")]
    public partial class qrtz_simple_triggers
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(120)]
        public string SCHED_NAME { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(200)]
        public string TRIGGER_NAME { get; set; }

        [Key]
        [Column(Order = 2)]
        [StringLength(200)]
        public string TRIGGER_GROUP { get; set; }

        public long REPEAT_COUNT { get; set; }

        public long REPEAT_INTERVAL { get; set; }

        public long TIMES_TRIGGERED { get; set; }

        public virtual qrtz_triggers qrtz_triggers { get; set; }
    }
}
