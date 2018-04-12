namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.commontg")]
    public partial class commontg
    {
        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string TgName { get; set; }

        public int idSwitch { get; set; }

        [Required]
        [StringLength(20)]
        public string description { get; set; }
    }
}
