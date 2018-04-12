namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.xyzprefix")]
    public partial class xyzprefix
    {
        [Key]
        [StringLength(25)]
        public string Prefix { get; set; }

        [StringLength(45)]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        public string CountryCode { get; set; }

        public DateTime date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int? field3 { get; set; }

        [StringLength(45)]
        public string field4 { get; set; }

        [StringLength(45)]
        public string field5 { get; set; }

        public float? refasr { get; set; }

        public float? refacd { get; set; }

        public float? refccr { get; set; }

        public float? refccrbycc { get; set; }

        public float? refpdd { get; set; }

        public float? refasrfas { get; set; }
    }
}
