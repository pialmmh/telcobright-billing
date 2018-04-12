namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.timezone")]
    public partial class timezone
    {
        public int zone_id { get; set; }

        [Required]
        [StringLength(6)]
        public string abbreviation { get; set; }

        public int time_start { get; set; }

        public int gmt_offset { get; set; }

        [Column(TypeName = "char")]
        [Required]
        [StringLength(1)]
        public string dst { get; set; }

        [StringLength(50)]
        public string offsetdesc { get; set; }

        public int id { get; set; }

        public virtual zone zone { get; set; }
    }
}
