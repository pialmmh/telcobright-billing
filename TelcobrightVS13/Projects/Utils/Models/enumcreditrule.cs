namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.enumcreditrule")]
    public partial class enumcreditrule
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }

        [Required]
        [StringLength(200)]
        public string RuleName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public int? json { get; set; }
    }
}
