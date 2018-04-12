namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.process")]
    public partial class process
    {
        public int id { get; set; }

        [Required]
        [StringLength(45)]
        public string ProcessName { get; set; }

        public DateTime? LastRun { get; set; }

        [StringLength(1073741823)]
        public string ProcessParamaterJson { get; set; }

        public int AdminState { get; set; }
    }
}
