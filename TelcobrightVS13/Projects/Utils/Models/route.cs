namespace Utils.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("platinum.route")]
    public partial class route
    {
        [Key]
        public int idroute { get; set; }

        [Required]
        [StringLength(45)]
        public string RouteName { get; set; }

        public int SwitchId { get; set; }

        public int CommonRoute { get; set; }

        public int idPartner { get; set; }

        public int NationalOrInternational { get; set; }

        [StringLength(45)]
        public string Description { get; set; }

        public int Status { get; set; }

        public DateTime? date1 { get; set; }

        public int? field1 { get; set; }

        public int? field2 { get; set; }

        public int? field3 { get; set; }

        public int? field4 { get; set; }

        [StringLength(45)]
        public string field5 { get; set; }

        public virtual partner partner { get; set; }
    }
}
