namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.ratetaskassignreference")]
    public partial class ratetaskassignreference
    {
        public int id { get; set; }

        public long? idRatePlan { get; set; }

        [StringLength(300)]
        public string Description { get; set; }
    }
}
