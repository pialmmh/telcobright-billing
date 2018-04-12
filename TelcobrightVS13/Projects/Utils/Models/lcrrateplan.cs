namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.lcrrateplan")]
    public partial class lcrrateplan
    {
        public long id { get; set; }

        public int idRatePlan { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        public DateTime StartDate { get; set; }

        public int JobCreated { get; set; }
    }
}
