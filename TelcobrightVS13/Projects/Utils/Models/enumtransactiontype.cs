namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.enumtransactiontype")]
    public partial class enumtransactiontype
    {
        public int id { get; set; }

        [Required]
        [StringLength(45)]
        public string Type { get; set; }
    }
}
