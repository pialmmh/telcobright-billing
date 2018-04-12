namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.qrtz_simprop_triggers")]
    public partial class qrtz_simprop_triggers
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

        [StringLength(512)]
        public string STR_PROP_1 { get; set; }

        [StringLength(512)]
        public string STR_PROP_2 { get; set; }

        [StringLength(512)]
        public string STR_PROP_3 { get; set; }

        public int? INT_PROP_1 { get; set; }

        public int? INT_PROP_2 { get; set; }

        public long? LONG_PROP_1 { get; set; }

        public long? LONG_PROP_2 { get; set; }

        public decimal? DEC_PROP_1 { get; set; }

        public decimal? DEC_PROP_2 { get; set; }

        public bool? BOOL_PROP_1 { get; set; }

        public bool? BOOL_PROP_2 { get; set; }

        public virtual qrtz_triggers qrtz_triggers { get; set; }
    }
}
