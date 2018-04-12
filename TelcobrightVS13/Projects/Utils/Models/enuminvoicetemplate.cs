namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.enuminvoicetemplate")]
    public partial class enuminvoicetemplate
    {
        public int id { get; set; }

        [Required]
        [StringLength(200)]
        public string TemplateName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public int? json { get; set; }

        [StringLength(500)]
        public string OtherInfo { get; set; }
    }
}
