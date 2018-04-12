namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.errordefinition")]
    public partial class errordefinition
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int idError { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public int Severity { get; set; }

        [StringLength(200)]
        public string Action { get; set; }
    }
}
