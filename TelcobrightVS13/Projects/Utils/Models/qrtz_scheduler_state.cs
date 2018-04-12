namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.qrtz_scheduler_state")]
    public partial class qrtz_scheduler_state
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(120)]
        public string SCHED_NAME { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(200)]
        public string INSTANCE_NAME { get; set; }

        public long LAST_CHECKIN_TIME { get; set; }

        public long CHECKIN_INTERVAL { get; set; }
    }
}
