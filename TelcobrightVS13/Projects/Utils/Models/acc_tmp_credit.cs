namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.acc_tmp_credit")]
    public partial class acc_tmp_credit
    {
        public long id { get; set; }

        public long externalId { get; set; }

        public DateTime starttime { get; set; }

        public int idpartner { get; set; }

        [Required]
        [StringLength(200)]
        public string TargetAccountName { get; set; }

        public double quantity { get; set; }

        public double? param1 { get; set; }

        public double? param2 { get; set; }

        [StringLength(400)]
        public string jsonDetail { get; set; }

        public int jobExecutionPriority { get; set; }

        public int? processed { get; set; }
    }
}
