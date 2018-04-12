namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.product")]
    public partial class product
    {
        public int id { get; set; }

        [StringLength(50)]
        public string Prefix { get; set; }

        [Required]
        [StringLength(45)]
        public string Name { get; set; }

        [StringLength(100)]
        public string description { get; set; }

        public int Category { get; set; }

        public int? SubCategory { get; set; }

        public long ServiceFamily { get; set; }

        public int? AccountingId { get; set; }
    }
}
