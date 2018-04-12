namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.enumdateparsestring")]
    public partial class enumdateparsestring
    {
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        public string value { get; set; }

        [Required]
        [StringLength(1073741823)]
        public string ParseString { get; set; }
    }
}
