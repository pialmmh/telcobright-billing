namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.lcr")]
    public partial class lcr
    {
        public long id { get; set; }

        [Required]
        [StringLength(25)]
        public string Prefix { get; set; }

        public long idrateplan { get; set; }

        public DateTime startdate { get; set; }

        [StringLength(500)]
        public string LcrCurrent { get; set; }

        [StringLength(1000)]
        public string LcrHistory { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime LastUpdated { get; set; }
    }
}
