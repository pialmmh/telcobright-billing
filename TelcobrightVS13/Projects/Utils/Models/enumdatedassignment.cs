namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.enumdatedassignment")]
    public partial class enumdatedassignment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }

        [Required]
        [StringLength(45)]
        public string Type { get; set; }

        [Required]
        [StringLength(200)]
        public string Tuple { get; set; }

        [StringLength(200)]
        public string description { get; set; }
    }
}
