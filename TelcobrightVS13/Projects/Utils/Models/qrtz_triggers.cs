namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.qrtz_triggers")]
    public partial class qrtz_triggers
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

        [Required]
        [StringLength(200)]
        public string JOB_NAME { get; set; }

        [Required]
        [StringLength(200)]
        public string JOB_GROUP { get; set; }

        [StringLength(250)]
        public string DESCRIPTION { get; set; }

        public long? NEXT_FIRE_TIME { get; set; }

        public long? PREV_FIRE_TIME { get; set; }

        public int? PRIORITY { get; set; }

        [Required]
        [StringLength(16)]
        public string TRIGGER_STATE { get; set; }

        [Required]
        [StringLength(8)]
        public string TRIGGER_TYPE { get; set; }

        public long START_TIME { get; set; }

        public long? END_TIME { get; set; }

        [StringLength(200)]
        public string CALENDAR_NAME { get; set; }

        public short? MISFIRE_INSTR { get; set; }

        [Column(TypeName = "blob")]
        public byte[] JOB_DATA { get; set; }

        public virtual qrtz_blob_triggers qrtz_blob_triggers { get; set; }

        public virtual qrtz_cron_triggers qrtz_cron_triggers { get; set; }

        public virtual qrtz_job_details qrtz_job_details { get; set; }

        public virtual qrtz_simple_triggers qrtz_simple_triggers { get; set; }

        public virtual qrtz_simprop_triggers qrtz_simprop_triggers { get; set; }
    }
}
