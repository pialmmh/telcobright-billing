namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.ansprefixextra")]
    public partial class ansprefixextra
    {
        public int id { get; set; }

        [Required]
        [StringLength(20)]
        public string PrefixBeforeAnsNumber { get; set; }
    }
}
