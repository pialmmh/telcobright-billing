namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.qrtz_fired_triggers")]
    public partial class qrtz_fired_triggers
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(120)]
        public string SCHED_NAME { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(95)]
        public string ENTRY_ID { get; set; }

        [Required]
        [StringLength(200)]
        public string TRIGGER_NAME { get; set; }

        [Required]
        [StringLength(200)]
        public string TRIGGER_GROUP { get; set; }

        [Required]
        [StringLength(200)]
        public string INSTANCE_NAME { get; set; }

        public long FIRED_TIME { get; set; }

        public long SCHED_TIME { get; set; }

        public int PRIORITY { get; set; }

        [Required]
        [StringLength(16)]
        public string STATE { get; set; }

        [StringLength(200)]
        public string JOB_NAME { get; set; }

        [StringLength(200)]
        public string JOB_GROUP { get; set; }

        public bool? IS_NONCONCURRENT { get; set; }

        public bool? REQUESTS_RECOVERY { get; set; }
    }
}
