namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.enumservicefamily")]
    public partial class enumservicefamily
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }

        [Required]
        [StringLength(200)]
        public string ServiceName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        public int? PartnerAssignNotNeeded { get; set; }

        public int ServiceCategory { get; set; }

        public int? AccountingId { get; set; }

        public virtual enumservicecategory enumservicecategory { get; set; }
    }
}
