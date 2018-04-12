namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.reporttemplate")]
    public partial class reporttemplate
    {
        [Key]
        [StringLength(200)]
        public string Templatename { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [StringLength(65535)]
        public string PageUrl { get; set; }

        [Column(TypeName = "text")]
        [StringLength(65535)]
        public string ControlValues { get; set; }
    }
}
