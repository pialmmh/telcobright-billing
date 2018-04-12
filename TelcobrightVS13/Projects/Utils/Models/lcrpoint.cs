namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.lcrpoint")]
    public partial class lcrpoint
    {
        public long id { get; set; }

        public DateTime TriggerTime { get; set; }

        [Required]
        [StringLength(5)]
        public string RateChangeType { get; set; }

        [Required]
        [StringLength(100)]
        public string prefix { get; set; }

        public int JobCreated { get; set; }

        public int RatePlanAssignmentFlag { get; set; }
    }
}
