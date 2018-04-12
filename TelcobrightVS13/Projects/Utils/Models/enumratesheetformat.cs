namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.enumratesheetformat")]
    public partial class enumratesheetformat
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }

        [Required]
        [StringLength(45)]
        public string Type { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string IdentifierTextJson { get; set; }
    }
}
