namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.rateplanassign")]
    public partial class rateplanassign
    {
        public long id { get; set; }

        public int Type { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public DateTime date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int? field3 { get; set; }

        [StringLength(45)]
        public string field4 { get; set; }

        [StringLength(45)]
        public string field5 { get; set; }

        public int TimeZone { get; set; }

        public int idCarrier { get; set; }

        public int Currency { get; set; }

        public DateTime? codedeletedate { get; set; }

        public int? ChangeCommitted { get; set; }

        [Required]
        [StringLength(200)]
        public string RatePlanName { get; set; }

        public int Resolution { get; set; }

        public int MinDurationSec { get; set; }

        public int SurchargeTime { get; set; }

        public double SurchargeAmount { get; set; }

        public sbyte? Category { get; set; }

        public sbyte? SubCategory { get; set; }
    }
}
