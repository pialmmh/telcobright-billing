namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.countrycode")]
    public partial class countrycode
    {
        [Key]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(45)]
        public string Name { get; set; }

        public float? refasr { get; set; }

        public float? refacd { get; set; }

        public float? refccr { get; set; }

        public float? refccrbycc { get; set; }

        public float? refpdd { get; set; }

        public float? refasrfas { get; set; }
    }
}
