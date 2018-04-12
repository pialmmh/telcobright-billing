namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.xyzselected")]
    public partial class xyzselected
    {
        public int id { get; set; }

        [Required]
        [StringLength(45)]
        public string prefix { get; set; }

        public int PrefixSet { get; set; }
    }
}
