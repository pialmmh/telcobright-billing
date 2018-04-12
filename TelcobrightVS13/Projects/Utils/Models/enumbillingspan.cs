namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.enumbillingspan")]
    public partial class enumbillingspan
    {
        public int id { get; set; }

        [Required]
        [StringLength(45)]
        public string Type { get; set; }

        public long value { get; set; }

        [StringLength(10)]
        public string ofbiz_uom_Id { get; set; }
    }
}
